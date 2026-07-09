using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Finance;

namespace FoodstoreApi.Usecase.Interfaces;

public interface ILedgerService
{
    /// <summary>Stages a balanced, idempotent payment journal in the current unit of work.</summary>
    Task StagePaymentConfirmationAsync(PaymentIntent intent, Order order, CancellationToken ct = default);
    Task StageRefundAsync(PaymentIntent intent, Order order, CancellationToken ct = default);
    Task StageSettlementPayoutAsync(SettlementBatch batch, CancellationToken ct = default);
}
