using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FoodstoreApi.Infrastructure.Repositories;
public sealed class FinanceOperationsRepository(StoreDbContext context) : IFinanceOperationsRepository
{
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
