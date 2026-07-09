using FoodstoreApi.Core.Configuration;
using FoodstoreApi.Core.Constants;
using FoodstoreApi.Core.Entities;
using FoodstoreApi.Usecase.DTOs.Payment;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodstoreApi.Usecase.Services;

public class PaymentService(
    IPaymentIntentRepository intentRepo,
    IPaymentEventRepository eventRepo,
    IOrderRepository orderRepo,
    ICustomerService customerService,
    IPaymentProviderFactory providerFactory,
    IOptions<IntaSendOptions> options,
    ILogger<PaymentService> logger) : IPaymentService
{
    private readonly IPaymentIntentRepository _intentRepo = intentRepo;
    private readonly IPaymentEventRepository _eventRepo = eventRepo;
    private readonly IOrderRepository _orderRepo = orderRepo;
    private readonly ICustomerService _customerService = customerService;
    private readonly IPaymentProviderFactory _providerFactory = providerFactory;
    private readonly IntaSendOptions _options = options.Value;
    private readonly ILogger<PaymentService> _logger = logger;

    private const decimal AmountTolerance = 0.01m;

    public async Task<PaymentIntentDto> CreateIntentAsync(CreatePaymentIntentDto dto, string idempotencyKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new InvalidOperationException("An idempotency key is required.");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new InvalidOperationException("A customer phone number is required.");

        // Repeated submissions with the same key return the original intent — never a second charge.
        var existingByKey = await _intentRepo.GetByIdempotencyKeyAsync(idempotencyKey, ct);
        if (existingByKey is not null)
            return MapToDto(existingByKey);

        var order = await _orderRepo.GetByIdAsync(dto.OrderId, ct)
            ?? throw new InvalidOperationException("Order was not found.");
        if (order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Order is not awaiting payment (status: {order.Status}).");

        // Avoid duplicate STK pushes if an attempt is already in flight for this order.
        var openIntent = await _intentRepo.GetActiveByOrderIdAsync(order.Id, ct);
        if (openIntent is not null && PaymentStateMachine.IsOpen(openIntent.Status))
            return MapToDto(openIntent);

        var provider = _providerFactory.GetProvider(PaymentProviders.IntaSend)
            ?? throw new InvalidOperationException("Payment provider is not available.");

        var intent = new PaymentIntent
        {
            OrderId = order.Id,
            BranchId = order.BranchId,
            Amount = order.TotalAmount ?? 0m,
            Currency = _options.DefaultCurrency,
            Provider = PaymentProviders.IntaSend,
            IdempotencyKey = idempotencyKey,
            Status = PaymentIntentStatus.Created,
            CustomerPhone = dto.Phone,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };
        await _intentRepo.AddAsync(intent, ct);

        var charge = await provider.CreateChargeAsync(new CreateChargeRequest(
            intent.Id, order.Id, order.OrderCode, intent.Amount, intent.Currency,
            dto.Phone, dto.CustomerName, dto.CustomerEmail), ct);

        if (charge.Success)
        {
            intent.ProviderReference = charge.ProviderReference;
            intent.CheckoutId = charge.CheckoutId;
            intent.Status = PaymentIntentStatus.Pending;
        }
        else
        {
            intent.Status = PaymentIntentStatus.Failed;
            intent.FailureReason = charge.ErrorMessage;
            _logger.LogWarning("Payment intent {IntentId} charge failed: {Reason}", intent.Id, charge.ErrorMessage);
        }
        await _intentRepo.UpdateAsync(intent, ct);

        return MapToDto(intent);
    }

    public async Task<PaymentIntentDto?> GetIntentAsync(Guid id, CancellationToken ct = default)
    {
        var intent = await _intentRepo.GetByIdAsync(id, ct);
        return intent is null ? null : MapToDto(intent);
    }

    public async Task<PaymentIntentDto?> GetIntentByOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var intent = await _intentRepo.GetActiveByOrderIdAsync(orderId, ct);
        return intent is null ? null : MapToDto(intent);
    }

    public async Task<PaymentIntentDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default)
    {
        var intent = await _intentRepo.GetByIdWithEventsAsync(id, ct);
        if (intent is null) return null;

        var dto = new PaymentIntentDetailDto();
        MapInto(intent, dto);
        dto.OrderCode = intent.Order?.OrderCode;
        dto.Events = intent.Events
            .OrderByDescending(e => e.ReceivedAt)
            .Select(MapEvent)
            .ToList();
        return dto;
    }

    public async Task<(IReadOnlyList<PaymentIntentDto> Items, int TotalCount)> SearchAsync(PaymentSearchFilter filter, CancellationToken ct = default)
    {
        var (items, total) = await _intentRepo.SearchAsync(
            filter.OrderId, filter.OrderCode, filter.Phone, filter.ProviderReference,
            filter.Status, filter.FromDate, filter.ToDate,
            Math.Max(1, filter.Page), Math.Clamp(filter.PageSize, 1, 200), ct);

        var dtos = items.Select(i =>
        {
            var d = MapToDto(i);
            d.OrderCode = i.Order?.OrderCode;
            return d;
        }).ToList();
        return (dtos, total);
    }

    public async Task<PaymentIntentDto> RefundAsync(Guid intentId, string reason, CancellationToken ct = default)
    {
        var intent = await _intentRepo.GetByIdAsync(intentId, ct)
            ?? throw new InvalidOperationException("Payment intent was not found.");
        if (intent.Status != PaymentIntentStatus.Succeeded)
            throw new InvalidOperationException($"Only succeeded payments can be refunded (status: {intent.Status}).");
        if (string.IsNullOrWhiteSpace(intent.ProviderReference))
            throw new InvalidOperationException("Payment intent has no provider reference to refund.");

        var provider = _providerFactory.GetProvider(intent.Provider)
            ?? throw new InvalidOperationException("Payment provider is not available.");

        var result = await provider.RefundAsync(
            new RefundRequest(intent.ProviderReference, intent.Amount, intent.Currency, reason), ct);
        if (!result.Success)
            throw new InvalidOperationException($"Refund request failed: {result.ErrorMessage}");

        intent.Status = PaymentIntentStatus.RefundPending;
        await _intentRepo.UpdateAsync(intent, ct);
        return MapToDto(intent);
    }

    public async Task<PaymentEventProcessingResult> IngestWebhookAsync(
        string providerType, string rawBody, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default)
    {
        var provider = _providerFactory.GetProvider(providerType);
        if (provider is null)
            return new PaymentEventProcessingResult("unknown-provider");

        var validated = provider.ValidateWebhook(new WebhookValidationInput(rawBody, headers));

        if (!validated.IsValid || string.IsNullOrEmpty(validated.ProviderEventId))
        {
            _logger.LogWarning("Rejected {Provider} webhook: {Error}", providerType, validated.Error);
            // Store an audit trail of the rejected attempt without allowing it to affect state.
            await SafeStoreRejectedAsync(providerType, validated, rawBody, ct);
            return new PaymentEventProcessingResult("rejected");
        }

        // Replay protection: the same provider event id is only ever stored once.
        if (await _eventRepo.ExistsByProviderEventIdAsync(providerType, validated.ProviderEventId, ct))
            return new PaymentEventProcessingResult("duplicate-ignored");

        PaymentIntent? intent = null;
        if (!string.IsNullOrEmpty(validated.ProviderReference))
            intent = await _intentRepo.GetByProviderReferenceAsync(providerType, validated.ProviderReference, ct);

        var evt = new PaymentEvent
        {
            PaymentIntentId = intent?.Id,
            Provider = providerType,
            ProviderEventId = validated.ProviderEventId,
            EventType = validated.Status ?? "unknown",
            SignatureValid = true,
            PayloadJson = rawBody,
            ReceivedAt = DateTime.UtcNow,
            ProcessingOutcome = intent is null ? "unmatched" : null,
            ProcessedAt = intent is null ? DateTime.UtcNow : null
        };
        try
        {
            await _eventRepo.AddAsync(evt, ct);
        }
        catch (Exception ex)
        {
            // Unique index race: another delivery stored it first — treat as duplicate.
            _logger.LogInformation(ex, "Duplicate {Provider} event {EventId} ignored", providerType, validated.ProviderEventId);
            return new PaymentEventProcessingResult("duplicate-ignored");
        }

        return new PaymentEventProcessingResult(intent is null ? "unmatched" : "queued", intent?.Id);
    }

    public async Task<PaymentEventProcessingResult> HandleProviderEventAsync(PaymentEvent evt, CancellationToken ct = default)
    {
        if (evt.PaymentIntentId is null)
            return await MarkProcessed(evt, "unmatched", ct);

        var intent = await _intentRepo.GetByIdAsync(evt.PaymentIntentId.Value, ct);
        if (intent is null)
            return await MarkProcessed(evt, "unmatched", ct);

        var target = evt.EventType;

        // Terminal / non-advancing states are idempotent no-ops.
        if (!PaymentStateMachine.CanTransition(intent.Status, target))
            return await MarkProcessed(evt, "duplicate-ignored", ct, intent.Id, intent.OrderId, intent.Status);

        return target switch
        {
            PaymentIntentStatus.Succeeded => await ConfirmSuccessAsync(intent, evt, ct),
            PaymentIntentStatus.Failed or PaymentIntentStatus.Expired or PaymentIntentStatus.Cancelled
                => await ApplyFailureAsync(intent, evt, target, ct),
            PaymentIntentStatus.Refunded => await ApplyRefundedAsync(intent, evt, ct),
            _ => await MarkProcessed(evt, "ignored", ct, intent.Id, intent.OrderId, intent.Status)
        };
    }

    public async Task<IReadOnlyList<PaymentEventProcessingResult>> PollPendingAsync(CancellationToken ct = default)
    {
        var results = new List<PaymentEventProcessingResult>();

        // 1) Drain any stored-but-unprocessed webhook events (durable queue).
        foreach (var evt in await _eventRepo.GetUnprocessedAsync(50, ct))
        {
            try
            {
                results.Add(await HandleProviderEventAsync(evt, ct));
            }
            catch (Exception ex)
            {
                evt.Attempts++;
                await _eventRepo.UpdateAsync(evt, ct);
                _logger.LogError(ex, "Failed to process payment event {EventId} (attempt {Attempts})", evt.Id, evt.Attempts);
            }
        }

        // 2) Reconcile stale pending intents by asking the provider directly.
        var cutoff = DateTime.UtcNow.AddSeconds(-15);
        foreach (var intent in await _intentRepo.GetStalePendingAsync(cutoff, 25, ct))
        {
            if (string.IsNullOrEmpty(intent.ProviderReference)) continue;
            var provider = _providerFactory.GetProvider(intent.Provider);
            if (provider is null) continue;

            var status = await provider.GetStatusAsync(intent.ProviderReference, ct);
            if (!status.Success || PaymentStateMachine.IsOpen(status.Status))
                continue; // still pending — leave it

            if (status.Status == PaymentIntentStatus.Succeeded
                && ((status.Amount.HasValue && Math.Abs(status.Amount.Value - intent.Amount) > AmountTolerance)
                    || (!string.IsNullOrWhiteSpace(status.Currency)
                        && !status.Currency.Equals(intent.Currency, StringComparison.OrdinalIgnoreCase))))
            {
                _logger.LogWarning("Provider status mismatch for intent {IntentId}", intent.Id);
                continue;
            }

            var eventId = $"{intent.ProviderReference}:{status.Status}".ToLowerInvariant();
            if (await _eventRepo.ExistsByProviderEventIdAsync(intent.Provider, eventId, ct))
                continue;

            var synthesized = new PaymentEvent
            {
                PaymentIntentId = intent.Id,
                Provider = intent.Provider,
                ProviderEventId = eventId,
                EventType = status.Status,
                SignatureValid = true,
                PayloadJson = status.RawResponse ?? "{}",
                ReceivedAt = DateTime.UtcNow
            };
            try
            {
                await _eventRepo.AddAsync(synthesized, ct);
                results.Add(await HandleProviderEventAsync(synthesized, ct));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reconcile intent {IntentId}", intent.Id);
            }
        }

        return results;
    }

    // --- state transitions ---

    private async Task<PaymentEventProcessingResult> ConfirmSuccessAsync(PaymentIntent intent, PaymentEvent evt, CancellationToken ct)
    {
        // Trust only provider-confirmed amount/currency.
        var validation = ValidateAmount(intent, evt);
        if (validation is not null)
            return await MarkProcessed(evt, validation, ct, intent.Id, intent.OrderId, intent.Status);

        var order = await _orderRepo.GetByIdAsync(intent.OrderId, ct);
        if (order is null)
            return await MarkProcessed(evt, "order-missing", ct, intent.Id);

        var now = DateTime.UtcNow;
        intent.Status = PaymentIntentStatus.Succeeded;

        Payment? payment = null;
        OrderStatusHistory? history = null;
        if (order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Paid;
            order.PaidAt = now;
            payment = new Payment
            {
                OrderId = order.Id,
                Amount = intent.Amount,
                Method = PaymentMethods.Mpesa,
                ReferenceCode = intent.ProviderReference,
                Status = "success",
                PaidAt = now
            };
            history = new OrderStatusHistory
            {
                OrderId = order.Id,
                FromStatus = OrderStatus.Pending,
                ToStatus = OrderStatus.Paid,
                ChangedAt = now,
                Note = $"Payment confirmed via {intent.Provider} ({intent.ProviderReference})"
            };
        }

        evt.ProcessedAt = now;
        evt.ProcessingOutcome = "applied";

        await _intentRepo.PersistConfirmationAsync(intent, evt, order, payment, history, ct);

        // Loyalty is non-critical and only runs once (guarded by the terminal-state check above).
        if (payment is not null && order.CustomerId.HasValue && order.TotalAmount.HasValue)
        {
            try
            {
                var points = (int)(order.TotalAmount.Value / 1000);
                if (points > 0)
                    await _customerService.AddPointsAsync(order.CustomerId.Value, points, $"Order {order.OrderCode} paid");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Loyalty accrual failed for order {OrderId}", order.Id);
            }
        }

        _logger.LogInformation("Payment intent {IntentId} succeeded; order {OrderId} marked paid", intent.Id, order.Id);
        return new PaymentEventProcessingResult("applied", intent.Id, order.Id, order.Status, Confirmed: true);
    }

    private async Task<PaymentEventProcessingResult> ApplyFailureAsync(PaymentIntent intent, PaymentEvent evt, string target, CancellationToken ct)
    {
        intent.Status = target;
        intent.FailureReason ??= ExtractFailureReason(evt);
        await _intentRepo.UpdateAsync(intent, ct);
        return await MarkProcessed(evt, "applied", ct, intent.Id, intent.OrderId, intent.Status);
    }

    private async Task<PaymentEventProcessingResult> ApplyRefundedAsync(PaymentIntent intent, PaymentEvent evt, CancellationToken ct)
    {
        intent.Status = PaymentIntentStatus.Refunded;
        await _intentRepo.UpdateAsync(intent, ct);
        return await MarkProcessed(evt, "applied", ct, intent.Id, intent.OrderId, intent.Status);
    }

    private string? ValidateAmount(PaymentIntent intent, PaymentEvent evt)
    {
        // Amount/currency are re-derived from the trusted payload the provider already validated.
        // The event payload is authoritative; if the provider omitted them we accept the intent amount.
        var provider = _providerFactory.GetProvider(intent.Provider);
        if (provider is null) return "provider-missing";

        var validated = provider.ValidateWebhook(new WebhookValidationInput(evt.PayloadJson, new Dictionary<string, string>()));
        // Synthesized poll events won't carry a challenge; only enforce when amount is present.
        if (validated.Amount.HasValue && Math.Abs(validated.Amount.Value - intent.Amount) > AmountTolerance)
        {
            _logger.LogWarning("Amount mismatch on intent {IntentId}: expected {Expected}, got {Actual}",
                intent.Id, intent.Amount, validated.Amount);
            return "amount-mismatch";
        }
        if (!string.IsNullOrEmpty(validated.Currency)
            && !validated.Currency.Equals(intent.Currency, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Currency mismatch on intent {IntentId}: expected {Expected}, got {Actual}",
                intent.Id, intent.Currency, validated.Currency);
            return "currency-mismatch";
        }
        return null;
    }

    private static string? ExtractFailureReason(PaymentEvent evt)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(evt.PayloadJson);
            if (doc.RootElement.TryGetProperty("failed_reason", out var r) && r.ValueKind == System.Text.Json.JsonValueKind.String)
                return r.GetString();
        }
        catch { /* ignore */ }
        return null;
    }

    private async Task<PaymentEventProcessingResult> MarkProcessed(
        PaymentEvent evt, string outcome, CancellationToken ct,
        Guid? intentId = null, Guid? orderId = null, string? orderStatus = null)
    {
        evt.ProcessedAt = DateTime.UtcNow;
        evt.ProcessingOutcome = outcome;
        await _eventRepo.UpdateAsync(evt, ct);
        return new PaymentEventProcessingResult(outcome, intentId, orderId, orderStatus);
    }

    private async Task SafeStoreRejectedAsync(string providerType, WebhookValidationResult validated, string rawBody, CancellationToken ct)
    {
        var eventId = validated.ProviderEventId ?? $"rejected:{Guid.NewGuid():N}";
        if (await _eventRepo.ExistsByProviderEventIdAsync(providerType, eventId, ct))
            return;
        try
        {
            await _eventRepo.AddAsync(new PaymentEvent
            {
                Provider = providerType,
                ProviderEventId = eventId,
                EventType = validated.EventType ?? "rejected",
                SignatureValid = false,
                PayloadJson = rawBody,
                ReceivedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow,
                ProcessingOutcome = "rejected"
            }, ct);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Could not store rejected webhook audit row");
        }
    }

    // --- mapping ---

    private static PaymentIntentDto MapToDto(PaymentIntent i)
    {
        var dto = new PaymentIntentDto();
        MapInto(i, dto);
        return dto;
    }

    private static void MapInto(PaymentIntent i, PaymentIntentDto dto)
    {
        dto.Id = i.Id;
        dto.OrderId = i.OrderId;
        dto.BranchId = i.BranchId;
        dto.Amount = i.Amount;
        dto.Currency = i.Currency;
        dto.Provider = i.Provider;
        dto.ProviderReference = i.ProviderReference;
        dto.CheckoutId = i.CheckoutId;
        dto.Status = i.Status;
        dto.CustomerPhone = i.CustomerPhone;
        dto.FailureReason = i.FailureReason;
        dto.ExpiresAt = i.ExpiresAt;
        dto.CreatedAt = i.CreatedAt;
        dto.UpdatedAt = i.UpdatedAt;
    }

    private static PaymentEventDto MapEvent(PaymentEvent e) => new()
    {
        Id = e.Id,
        Provider = e.Provider,
        ProviderEventId = e.ProviderEventId,
        EventType = e.EventType,
        SignatureValid = e.SignatureValid,
        ProcessingOutcome = e.ProcessingOutcome,
        Attempts = e.Attempts,
        ReceivedAt = e.ReceivedAt,
        ProcessedAt = e.ProcessedAt
    };
}
