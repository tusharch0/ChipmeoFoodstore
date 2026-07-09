using FoodstoreApi.Core.Entities;
using FoodstoreApi.Usecase.DTOs.Payment;

namespace FoodstoreApi.Usecase.Interfaces;

/// <summary>Outcome of ingesting or processing a single provider event, for callers that broadcast/notify.</summary>
public record PaymentEventProcessingResult(
    string Outcome,
    Guid? IntentId = null,
    Guid? OrderId = null,
    string? OrderStatus = null,
    bool Confirmed = false
);

public interface IPaymentService
{
    Task<PaymentIntentDto> CreateIntentAsync(CreatePaymentIntentDto dto, string idempotencyKey, CancellationToken ct = default);
    Task<PaymentIntentDto?> GetIntentAsync(Guid id, CancellationToken ct = default);
    Task<PaymentIntentDto?> GetIntentByOrderAsync(Guid orderId, CancellationToken ct = default);
    Task<PaymentIntentDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);

    Task<(IReadOnlyList<PaymentIntentDto> Items, int TotalCount)> SearchAsync(PaymentSearchFilter filter, CancellationToken ct = default);
    Task<PaymentIntentDto> RefundAsync(Guid intentId, string reason, CancellationToken ct = default);

    /// <summary>Validates and stores a raw webhook as a durable event. Does not apply state (the worker does).</summary>
    Task<PaymentEventProcessingResult> IngestWebhookAsync(string providerType, string rawBody, IReadOnlyDictionary<string, string> headers, CancellationToken ct = default);

    /// <summary>Applies one stored event to its intent/order idempotently.</summary>
    Task<PaymentEventProcessingResult> HandleProviderEventAsync(PaymentEvent evt, CancellationToken ct = default);

    /// <summary>Worker path: reconcile stale pending intents by polling the provider. Returns confirmations applied.</summary>
    Task<IReadOnlyList<PaymentEventProcessingResult>> PollPendingAsync(CancellationToken ct = default);
}
