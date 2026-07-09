using FoodstoreApi.Core.Entities;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IPaymentEventRepository
{
    Task<bool> ExistsByProviderEventIdAsync(string provider, string providerEventId, CancellationToken ct = default);
    Task<PaymentEvent> AddAsync(PaymentEvent evt, CancellationToken ct = default);
    Task UpdateAsync(PaymentEvent evt, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentEvent>> GetUnprocessedAsync(int max, CancellationToken ct = default);
    Task<IReadOnlyList<PaymentEvent>> GetByIntentIdAsync(Guid intentId, CancellationToken ct = default);
}
