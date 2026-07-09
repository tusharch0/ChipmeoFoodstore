using FoodstoreApi.Core.Entities;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IPaymentIntentRepository
{
    Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaymentIntent?> GetByIdWithEventsAsync(Guid id, CancellationToken ct = default);
    Task<PaymentIntent?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default);
    Task<PaymentIntent?> GetByProviderReferenceAsync(string provider, string providerReference, CancellationToken ct = default);
    Task<PaymentIntent?> GetActiveByOrderIdAsync(Guid orderId, CancellationToken ct = default);

    Task<PaymentIntent> AddAsync(PaymentIntent intent, CancellationToken ct = default);
    Task UpdateAsync(PaymentIntent intent, CancellationToken ct = default);

    Task<(IReadOnlyList<PaymentIntent> Items, int TotalCount)> SearchAsync(
        Guid? orderId, string? orderCode, string? phone, string? providerReference,
        string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize,
        CancellationToken ct = default);

    Task<IReadOnlyList<PaymentIntent>> GetStalePendingAsync(DateTime olderThanUtc, int max, CancellationToken ct = default);

    /// <summary>
    /// Persists a confirmed-payment graph (intent, event, order, settled payment, status history)
    /// in a single transaction so a payment cannot half-apply.
    /// </summary>
    Task PersistConfirmationAsync(
        PaymentIntent intent, PaymentEvent evt, Order order,
        Payment? payment, OrderStatusHistory? history, CancellationToken ct = default);
}
