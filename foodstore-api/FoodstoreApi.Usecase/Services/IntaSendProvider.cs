using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FoodstoreApi.Core.Configuration;
using FoodstoreApi.Core.Constants;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoodstoreApi.Usecase.Services;

/// <summary>
/// IntaSend M-Pesa payment adapter. Wraps the IntaSend Collection API (STK push, status, refund)
/// and validates inbound webhooks against the configured challenge. Degrades to a clear failure
/// result when credentials are absent so unconfigured environments do not throw.
/// </summary>
public class IntaSendProvider(
    HttpClient httpClient,
    IOptions<IntaSendOptions> options,
    ILogger<IntaSendProvider> logger) : IPaymentProvider
{
    private readonly HttpClient _http = httpClient;
    private readonly IntaSendOptions _options = options.Value;
    private readonly ILogger<IntaSendProvider> _logger = logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public string ProviderType => PaymentProviders.IntaSend;

    public async Task<CreateChargeResult> CreateChargeAsync(CreateChargeRequest request, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
            return new CreateChargeResult(false, null, null, null, "IntaSend is not configured.");

        try
        {
            var (firstName, lastName) = SplitName(request.CustomerName);
            var body = new Dictionary<string, object?>
            {
                ["public_key"] = _options.PublishableKey,
                ["amount"] = request.Amount,
                ["phone_number"] = NormalizePhone(request.CustomerPhone),
                ["currency"] = request.Currency,
                ["api_ref"] = request.OrderCode,
                ["first_name"] = firstName,
                ["last_name"] = lastName,
                ["email"] = request.CustomerEmail,
                ["narrative"] = $"Order {request.OrderCode}"
            };

            using var httpReq = BuildRequest(HttpMethod.Post, "/api/v1/payment/mpesa-stk-push/", body);
            using var resp = await _http.SendAsync(httpReq, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("IntaSend STK push failed ({Status}): {Body}", resp.StatusCode, raw);
                return new CreateChargeResult(false, null, null, raw, $"Provider returned {(int)resp.StatusCode}.");
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var invoiceId = TryGetString(root, "invoice", "invoice_id") ?? TryGetString(root, "invoice", "id");
            var trackingId = TryGetString(root, "id") ?? invoiceId;

            return new CreateChargeResult(true, invoiceId, trackingId, raw, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IntaSend CreateCharge error for order {OrderCode}", request.OrderCode);
            return new CreateChargeResult(false, null, null, null, ex.Message);
        }
    }

    public async Task<ProviderStatusResult> GetStatusAsync(string providerReference, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
            return new ProviderStatusResult(false, PaymentIntentStatus.Pending, null, null, providerReference, null, "IntaSend is not configured.");

        try
        {
            var body = new Dictionary<string, object?>
            {
                ["public_key"] = _options.PublishableKey,
                ["invoice_id"] = providerReference
            };

            using var httpReq = BuildRequest(HttpMethod.Post, "/api/v1/payment/status/", body);
            using var resp = await _http.SendAsync(httpReq, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                return new ProviderStatusResult(false, PaymentIntentStatus.Pending, null, null, providerReference, raw, $"Provider returned {(int)resp.StatusCode}.");

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var providerState = TryGetString(root, "invoice", "state") ?? TryGetString(root, "state") ?? "PENDING";
            var amount = TryGetDecimal(root, "invoice", "value") ?? TryGetDecimal(root, "invoice", "net_amount");
            var currency = TryGetString(root, "invoice", "currency");
            var failure = TryGetString(root, "invoice", "failed_reason");

            return new ProviderStatusResult(true, MapState(providerState), amount, currency, providerReference, raw, failure);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IntaSend GetStatus error for {Reference}", providerReference);
            return new ProviderStatusResult(false, PaymentIntentStatus.Pending, null, null, providerReference, null, ex.Message);
        }
    }

    public WebhookValidationResult ValidateWebhook(WebhookValidationInput input)
    {
        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(input.RawBody);
        }
        catch (Exception ex)
        {
            return Invalid($"Malformed JSON: {ex.Message}");
        }

        using (doc)
        {
            var root = doc.RootElement;

            // IntaSend authenticates webhooks with a shared challenge string in the payload.
            var challenge = TryGetString(root, "challenge");
            if (string.IsNullOrEmpty(_options.WebhookChallenge) || challenge != _options.WebhookChallenge)
                return Invalid("Webhook challenge mismatch.");

            var invoiceId = TryGetString(root, "invoice_id") ?? TryGetString(root, "invoice", "invoice_id");
            if (string.IsNullOrEmpty(invoiceId))
                return Invalid("Missing invoice_id.");

            var providerState = TryGetString(root, "state") ?? TryGetString(root, "invoice", "state") ?? "PENDING";
            var status = MapState(providerState);
            var amount = TryGetDecimal(root, "value") ?? TryGetDecimal(root, "invoice", "value");
            var currency = TryGetString(root, "currency") ?? TryGetString(root, "invoice", "currency");
            var failure = TryGetString(root, "failed_reason") ?? TryGetString(root, "invoice", "failed_reason");

            // Optional freshness check when the provider stamps the event.
            var updatedAt = TryGetString(root, "updated_at") ?? TryGetString(root, "created_at");
            if (!string.IsNullOrEmpty(updatedAt)
                && DateTimeOffset.TryParse(updatedAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var ts))
            {
                var age = DateTimeOffset.UtcNow - ts;
                if (age > TimeSpan.FromSeconds(_options.WebhookMaxAgeSeconds) && age > TimeSpan.Zero)
                    return Invalid("Webhook timestamp is stale.");
            }

            // Event id is stable per (invoice, state) so identical redeliveries dedupe while genuine
            // transitions (PENDING → COMPLETE) remain distinct events.
            var eventId = $"{invoiceId}:{providerState}".ToLowerInvariant();

            return new WebhookValidationResult(true, eventId, providerState, status, invoiceId, amount, currency, failure, null);
        }
    }

    public async Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
            return new RefundResult(false, null, null, "IntaSend is not configured.");

        try
        {
            var body = new Dictionary<string, object?>
            {
                ["invoice"] = request.ProviderReference,
                ["amount"] = request.Amount,
                ["reason"] = "Other",
                ["reason_details"] = request.Reason
            };

            using var httpReq = BuildRequest(HttpMethod.Post, "/api/v1/refunds/", body);
            using var resp = await _http.SendAsync(httpReq, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                return new RefundResult(false, null, raw, $"Provider returned {(int)resp.StatusCode}.");

            using var doc = JsonDocument.Parse(raw);
            var refundRef = TryGetString(doc.RootElement, "chargeback_id") ?? TryGetString(doc.RootElement, "id");
            return new RefundResult(true, refundRef, raw, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IntaSend Refund error for {Reference}", request.ProviderReference);
            return new RefundResult(false, null, null, ex.Message);
        }
    }

    public Task<IReadOnlyList<ProviderTransaction>> GetReconciliationAsync(DateOnly date, CancellationToken ct = default)
    {
        // Full reconciliation pull is delivered in the settlements phase; return empty until then.
        return Task.FromResult<IReadOnlyList<ProviderTransaction>>(Array.Empty<ProviderTransaction>());
    }

    // --- helpers ---

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, object body)
    {
        var req = new HttpRequestMessage(method, new Uri(new Uri(_options.BaseUrl), path))
        {
            Content = JsonContent.Create(body, options: JsonOpts)
        };
        req.Headers.TryAddWithoutValidation("Authorization", $"Bearer {_options.SecretKey}");
        req.Headers.TryAddWithoutValidation("X-IntaSend-Public-API-Key", _options.PublishableKey);
        return req;
    }

    private static string MapState(string providerState) => providerState?.Trim().ToUpperInvariant() switch
    {
        "COMPLETE" or "COMPLETED" or "PAID" => PaymentIntentStatus.Succeeded,
        "FAILED" => PaymentIntentStatus.Failed,
        "EXPIRED" => PaymentIntentStatus.Expired,
        "CANCELLED" or "CANCELED" => PaymentIntentStatus.Cancelled,
        "REFUNDED" => PaymentIntentStatus.Refunded,
        _ => PaymentIntentStatus.Pending
    };

    private static string NormalizePhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0")) digits = "254" + digits[1..];
        return digits;
    }

    private static (string First, string Last) SplitName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return ("Customer", "-");
        var parts = name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], "-");
    }

    private static WebhookValidationResult Invalid(string error) =>
        new(false, null, null, null, null, null, null, null, error);

    private static string? TryGetString(JsonElement root, params string[] path)
    {
        var el = root;
        foreach (var key in path)
        {
            if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(key, out el))
                return null;
        }
        return el.ValueKind switch
        {
            JsonValueKind.String => el.GetString(),
            JsonValueKind.Number => el.ToString(),
            JsonValueKind.True or JsonValueKind.False => el.ToString(),
            _ => null
        };
    }

    private static decimal? TryGetDecimal(JsonElement root, params string[] path)
    {
        var el = root;
        foreach (var key in path)
        {
            if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(key, out el))
                return null;
        }
        return el.ValueKind switch
        {
            JsonValueKind.Number when el.TryGetDecimal(out var d) => d,
            JsonValueKind.String when decimal.TryParse(el.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) => d,
            _ => null
        };
    }
}
