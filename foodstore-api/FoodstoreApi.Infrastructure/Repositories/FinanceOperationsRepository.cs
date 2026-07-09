using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Finance;
using Microsoft.EntityFrameworkCore;
namespace FoodstoreApi.Infrastructure.Repositories;
public sealed class FinanceOperationsRepository(StoreDbContext context) : IFinanceOperationsRepository
{
    public async Task<IReadOnlyList<FinanceExceptionDto>> UnmatchedPaymentsAsync(CancellationToken ct = default) =>
        await context.PaymentEvents.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.ProcessingOutcome == "unmatched")
            .OrderByDescending(item => item.ReceivedAt).Take(500)
            .Select(item => new FinanceExceptionDto(item.Id, "unmatched_payment", null, item.ProviderEventId, "unmatched", null,
                string.Empty, item.Provider + ": " + item.EventType, item.ReceivedAt)).ToListAsync(ct);

    public async Task<IReadOnlyList<FinanceExceptionDto>> ExceptionsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var branchIds = await context.Branches.IgnoreQueryFilters().Where(branch => branch.OrganizationId == organizationId).Select(branch => branch.Id).ToListAsync(ct);
        var paymentItems = await context.PaymentIntents.IgnoreQueryFilters().AsNoTracking()
            .Where(intent => intent.BranchId.HasValue && branchIds.Contains(intent.BranchId.Value) && (intent.Status == "failed" || intent.Status == "refund_pending"))
            .Select(intent => new FinanceExceptionDto(intent.Id, intent.Status == "failed" ? "payment_failure" : "refund_awaiting_action", intent.BranchId,
                intent.ProviderReference ?? intent.Id.ToString(), intent.Status, intent.Amount, intent.Currency, intent.FailureReason, intent.UpdatedAt)).ToListAsync(ct);
        var settlementItems = await context.SettlementBatches.AsNoTracking().Where(batch => batch.OrganizationId == organizationId && batch.Status == "failed")
            .Select(batch => new FinanceExceptionDto(batch.Id, "settlement_failure", null, batch.PayoutReference ?? batch.Id.ToString(), batch.Status,
                batch.NetAmount, batch.Currency, batch.FailureReason, batch.UpdatedAt)).ToListAsync(ct);
        var heldItems = await context.SettlementBatches.AsNoTracking().Where(batch => batch.OrganizationId == organizationId && batch.Holds > 0m && batch.Status != "paid" && batch.Status != "reconciled")
            .Select(batch => new FinanceExceptionDto(batch.Id, "held_wallet", null, batch.Id.ToString(), batch.Status,
                batch.Holds, batch.Currency, "Settlement is held by the minimum payout policy.", batch.UpdatedAt)).ToListAsync(ct);
        var reconciliationItems = await context.ReconciliationCases.AsNoTracking().Where(item => item.OrganizationId == organizationId && item.Status != "resolved")
            .Select(item => new FinanceExceptionDto(item.Id, "reconciliation_discrepancy", null, item.SourceReference, item.Status,
                item.ExternalAmount - item.InternalAmount, item.Currency, item.Resolution, item.CreatedAt)).ToListAsync(ct);
        var walletItems = await context.LedgerAccounts.AsNoTracking().Where(account => account.OrganizationId == organizationId && account.BranchId.HasValue && account.Code == "RESTAURANT_WALLET")
            .Select(account => new { Account = account, Balance = context.LedgerJournalLines.Where(line => line.AccountId == account.Id).Sum(line => (decimal?)(line.Credit - line.Debit)) ?? 0m })
            .Where(item => item.Balance < 0m)
            .Select(item => new FinanceExceptionDto(item.Account.Id, "negative_wallet", item.Account.BranchId, item.Account.Code, "open", item.Balance,
                item.Account.Currency, "Branch wallet balance is negative.", item.Account.UpdatedAt)).ToListAsync(ct);
        return paymentItems.Concat(settlementItems).Concat(heldItems).Concat(reconciliationItems).Concat(walletItems).OrderByDescending(item => item.OccurredAt).ToList();
    }
    public Task<FinanceAdjustmentRequest?> AdjustmentAsync(Guid id, CancellationToken ct = default) => context.FinanceAdjustmentRequests.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<FinanceAdjustmentRequest>> AdjustmentsAsync(Guid organizationId, CancellationToken ct = default) => await context.FinanceAdjustmentRequests.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    public async Task AddAdjustmentAsync(FinanceAdjustmentRequest request, CancellationToken ct = default) { await context.FinanceAdjustmentRequests.AddAsync(request, ct); await context.SaveChangesAsync(ct); }
    public async Task ApproveAdjustmentAsync(FinanceAdjustmentRequest request, CancellationToken ct = default)
    {
        var account = await context.LedgerAccounts.SingleAsync(x => x.Id == request.LedgerAccountId && x.OrganizationId == request.OrganizationId, ct);
        var offset = await context.LedgerAccounts.SingleOrDefaultAsync(x => x.OrganizationId == request.OrganizationId && x.Code == "ADJUSTMENT_OFFSET" && x.Currency == request.Currency, ct) ?? new LedgerAccount { OrganizationId = request.OrganizationId, Code = "ADJUSTMENT_OFFSET", Name = "Adjustment offset", AccountType = "equity", Currency = request.Currency, IsSystem = true };
        if (offset.Id == Guid.Empty) await context.LedgerAccounts.AddAsync(offset, ct);
        var amount = Math.Abs(request.Amount); var journal = new LedgerJournal { OrganizationId = request.OrganizationId, Currency = request.Currency, EntryType = "manual_adjustment", SourceType = "adjustment_request", SourceId = request.Id, Description = $"{request.ReasonCode}: {request.Reason}", PostedAt = DateTime.UtcNow, ActorId = request.ApprovedBy };
        journal.Lines.Add(new LedgerJournalLine { Account = request.Amount >= 0 ? account : offset, Debit = amount, Memo = request.Reason }); journal.Lines.Add(new LedgerJournalLine { Account = request.Amount >= 0 ? offset : account, Credit = amount, Memo = request.Reason });
        await context.LedgerJournals.AddAsync(journal, ct); request.JournalId = journal.Id; context.FinanceAdjustmentRequests.Update(request); await context.SaveChangesAsync(ct);
    }
    public Task<ReconciliationCase?> CaseAsync(Guid id, CancellationToken ct = default) => context.ReconciliationCases.SingleOrDefaultAsync(x => x.Id == id, ct);
    public async Task<IReadOnlyList<ReconciliationCase>> CasesAsync(Guid organizationId, CancellationToken ct = default) => await context.ReconciliationCases.AsNoTracking().Where(x => x.OrganizationId == organizationId).OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
    public async Task AddCaseAsync(ReconciliationCase item, CancellationToken ct = default) { await context.ReconciliationCases.AddAsync(item, ct); await context.SaveChangesAsync(ct); }
    public async Task SaveCaseAsync(ReconciliationCase item, CancellationToken ct = default) { context.ReconciliationCases.Update(item); await context.SaveChangesAsync(ct); }
}
