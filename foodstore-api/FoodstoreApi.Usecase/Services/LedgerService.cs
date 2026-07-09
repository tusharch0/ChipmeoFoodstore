using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Usecase.Interfaces;

namespace FoodstoreApi.Usecase.Services;

public sealed class LedgerService(ILedgerRepository repository) : ILedgerService
{
    public async Task StagePaymentConfirmationAsync(PaymentIntent intent, Order order, CancellationToken ct = default)
    {
        if (await repository.ExistsForSourceAsync("payment_intent", intent.Id, ct)) return;
        var branchId = intent.BranchId ?? order.BranchId
            ?? throw new InvalidOperationException("A branch-scoped payment is required for ledger posting.");
        var policy = await repository.GetPolicyAsync(branchId, intent.Amount, ct);
        if (!string.Equals(policy.Currency, intent.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Finance policy currency does not match the payment intent.");
        await repository.StagePaymentJournalAsync(intent, order, policy, ct);
    }

    public Task StageRefundAsync(PaymentIntent intent, Order order, CancellationToken ct = default) => repository.StageRefundJournalAsync(intent, order, ct);
    public Task StageSettlementPayoutAsync(SettlementBatch batch, CancellationToken ct = default) => repository.StageSettlementPayoutJournalAsync(batch, ct);
}
