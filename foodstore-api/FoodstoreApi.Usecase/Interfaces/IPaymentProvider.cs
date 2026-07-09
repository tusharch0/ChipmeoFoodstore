namespace FoodstoreApi.Usecase.Interfaces;

public record CreateChargeRequest(
    Guid PaymentIntentId,
    Guid OrderId,
    string OrderCode,
    decimal Amount,
    string Currency,
    string CustomerPhone,
    string? CustomerName,
    string? CustomerEmail
);

public record CreateChargeResult(
    bool Success,
    string? ProviderReference,
    string? CheckoutId,
    string? RawResponse,
    string? ErrorMessage
);

/// <summary>Normalized provider outcome. <see cref="Status"/> is a <c>PaymentIntentStatus</c> value.</summary>
public record ProviderStatusResult(
    bool Success,
    string Status,
    decimal? Amount,
    string? Currency,
    string? ProviderReference,
    string? RawResponse,
    string? FailureReason
);

public record WebhookValidationInput(
    string RawBody,
    IReadOnlyDictionary<string, string> Headers
);

/// <summary>Result of validating a raw webhook. When <see cref="IsValid"/> is false the payload is not trusted.</summary>
public record WebhookValidationResult(
    bool IsValid,
    string? ProviderEventId,
    string? EventType,
    string? Status,
    string? ProviderReference,
    decimal? Amount,
    string? Currency,
    string? FailureReason,
    string? Error
);

public record RefundRequest(
    string ProviderReference,
    decimal Amount,
    string Currency,
    string Reason
);

public record RefundResult(
    bool Success,
    string? RefundReference,
    string? RawResponse,
    string? ErrorMessage
);

public record ProviderTransaction(
    string ProviderReference,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt
);

/// <summary>
/// Application-level abstraction over a payment provider (e.g. IntaSend). Keeps provider HTTP,
/// signing, and payload shapes out of controllers, services, and UI. Mirrors <see cref="IEInvoiceProvider"/>.
/// </summary>
public interface IPaymentProvider
{
    string ProviderType { get; }

    Task<CreateChargeResult> CreateChargeAsync(CreateChargeRequest request, CancellationToken ct = default);

    Task<ProviderStatusResult> GetStatusAsync(string providerReference, CancellationToken ct = default);

    /// <summary>Validates a raw webhook (signature/challenge, freshness) and extracts a normalized event. Synchronous — no network.</summary>
    WebhookValidationResult ValidateWebhook(WebhookValidationInput input);

    Task<RefundResult> RefundAsync(RefundRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<ProviderTransaction>> GetReconciliationAsync(DateOnly date, CancellationToken ct = default);
}

public interface IPaymentProviderFactory
{
    /// <summary>Resolves the registered provider for the given type (e.g. <c>intasend</c>), or null.</summary>
    IPaymentProvider? GetProvider(string providerType);
}
