using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Usecase.Interfaces;
namespace FoodstoreApi.Usecase.Services;
public sealed class SettlementService(ISettlementRepository repository, ILedgerService ledgerService) : ISettlementService
{
    private static readonly Dictionary<string, string[]> Transitions = new() { ["draft"] = ["reviewed"], ["reviewed"] = ["approved"], ["approved"] = ["submitted"], ["submitted"] = ["paid", "failed", "partially_paid"], ["paid"] = ["reconciled"], ["partially_paid"] = ["reconciled"] };
    public Task<SettlementBatch?> GetAsync(Guid id, CancellationToken ct = default) => repository.GetAsync(id, ct);
    public Task<SettlementBatch> GenerateAsync(Guid organizationId, DateOnly date, string currency, Guid actorId, CancellationToken ct = default) => repository.GenerateAsync(organizationId, date, currency, actorId, ct);
    public async Task<SettlementBatch> TransitionAsync(Guid id, string target, Guid actorId, string? payoutReference, CancellationToken ct = default)
    {
        var batch = await repository.GetAsync(id, ct) ?? throw new InvalidOperationException("Settlement batch was not found.");
        if (!Transitions.TryGetValue(batch.Status, out var allowed) || !allowed.Contains(target)) throw new InvalidOperationException($"Cannot transition settlement from {batch.Status} to {target}.");
        if ((target == "reviewed" && batch.CreatedBy == actorId) || (target == "approved" && (batch.CreatedBy == actorId || batch.ReviewedBy == actorId))) throw new InvalidOperationException("Settlement requires a different maker and checker.");
        var now = DateTime.UtcNow; batch.Status = target;
        if (target == "reviewed") { batch.ReviewedBy = actorId; batch.ReviewedAt = now; } if (target == "approved") { batch.ApprovedBy = actorId; batch.ApprovedAt = now; } if (target is "paid" or "partially_paid") { batch.PaidBy = actorId; batch.PaidAt = now; batch.PayoutReference = payoutReference ?? throw new InvalidOperationException("A payout reference is required."); await ledgerService.StageSettlementPayoutAsync(batch, ct); }
        await repository.SaveAsync(batch, ct); return batch;
    }
    public Task<IReadOnlyList<SettlementBatch>> GetAllAsync(Guid organizationId, CancellationToken ct = default) => repository.GetAllAsync(organizationId, ct);
}
