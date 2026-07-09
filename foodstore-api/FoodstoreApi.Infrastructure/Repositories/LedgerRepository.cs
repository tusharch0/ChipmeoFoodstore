using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Finance;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public sealed class LedgerRepository(StoreDbContext context) : ILedgerRepository
{
    public Task<bool> ExistsForSourceAsync(string sourceType, Guid sourceId, CancellationToken ct = default) =>
        context.LedgerJournals.AsNoTracking().AnyAsync(j => j.SourceType == sourceType && j.SourceId == sourceId, ct);

    public async Task<FinancialPolicySnapshot> GetPolicyAsync(Guid branchId, decimal amount, CancellationToken ct = default)
    {
        var organizationId = await context.Branches.IgnoreQueryFilters().Where(b => b.Id == branchId).Select(b => b.OrganizationId).SingleAsync(ct);
        var policy = await context.FinancePolicies.SingleOrDefaultAsync(p => p.OrganizationId == organizationId && p.IsActive, ct);
        var currency = await context.Organizations.Where(o => o.Id == organizationId).Select(o => o.CurrencyCode).SingleAsync(ct);
        if (policy is null) return new FinancialPolicySnapshot(0m, 0m, "restaurant", currency);
        decimal Calculate(string mode, decimal value) => mode.Equals("percentage", StringComparison.OrdinalIgnoreCase) ? Math.Round(amount * value / 100m, 2, MidpointRounding.AwayFromZero) : value;
        return new FinancialPolicySnapshot(Calculate(policy.CommissionMode, policy.CommissionValue), Calculate(policy.ProviderFeeMode, policy.ProviderFeeValue), policy.FeeBearer, currency);
    }

    public async Task StagePaymentJournalAsync(PaymentIntent intent, Order order, FinancialPolicySnapshot policy, CancellationToken ct = default)
    {
        var organizationId = await context.Branches.IgnoreQueryFilters().Where(b => b.Id == intent.BranchId).Select(b => b.OrganizationId).SingleAsync(ct);
        var commission = Math.Clamp(policy.Commission, 0m, intent.Amount);
        var fee = Math.Clamp(policy.ProviderFee, 0m, intent.Amount - commission);
        var restaurantFee = policy.FeeBearer.Equals("restaurant", StringComparison.OrdinalIgnoreCase) ? fee : 0m;
        var walletCredit = intent.Amount - commission - restaurantFee;
        var providerClearing = await AccountAsync(null, "PROVIDER_CLEARING", "IntaSend provider clearing", "asset", policy.Currency, ct);
        var platformRevenue = await AccountAsync(null, "PLATFORM_COMMISSION", "Platform commission revenue", "revenue", policy.Currency, ct);
        var providerFees = await AccountAsync(null, "PROVIDER_FEES", "Provider fee expense", "expense", policy.Currency, ct);
        var wallet = await AccountAsync(organizationId, "RESTAURANT_WALLET", "Restaurant wallet payable", "liability", policy.Currency, ct);
        var journal = new LedgerJournal { OrganizationId = organizationId, BranchId = intent.BranchId, Currency = policy.Currency, EntryType = "payment_confirmed", SourceType = "payment_intent", SourceId = intent.Id, Description = $"Confirmed payment for order {order.OrderCode}", PostedAt = DateTime.UtcNow };
        Add(journal, providerClearing, intent.Amount, 0m, "Customer payment received");
        Add(journal, wallet, 0m, walletCredit, "Restaurant wallet payable");
        if (commission > 0m) Add(journal, platformRevenue, 0m, commission, "Platform commission");
        if (fee > 0m) { Add(journal, providerFees, fee, 0m, "Provider fee"); Add(journal, providerClearing, 0m, fee, "Provider fee deduction"); }
        if (journal.Lines.Sum(l => l.Debit) != journal.Lines.Sum(l => l.Credit)) throw new InvalidOperationException("Ledger journal is not balanced.");
        await context.LedgerJournals.AddAsync(journal, ct);
    }

    public async Task StageRefundJournalAsync(PaymentIntent intent, Order order, CancellationToken ct = default)
    {
        if (await ExistsForSourceAsync("payment_refund", intent.Id, ct)) return;
        var organizationId = await context.Branches.IgnoreQueryFilters().Where(b => b.Id == intent.BranchId).Select(b => b.OrganizationId).SingleAsync(ct);
        var clearing = await AccountAsync(null, "PROVIDER_CLEARING", "IntaSend provider clearing", "asset", intent.Currency, ct);
        var wallet = await AccountAsync(organizationId, "RESTAURANT_WALLET", "Restaurant wallet payable", "liability", intent.Currency, ct);
        var journal = new LedgerJournal { OrganizationId = organizationId, BranchId = intent.BranchId, Currency = intent.Currency, EntryType = "payment_refunded", SourceType = "payment_refund", SourceId = intent.Id, Description = $"Refund confirmed for order {order.OrderCode}", PostedAt = DateTime.UtcNow };
        Add(journal, wallet, intent.Amount, 0m, "Refund reduces restaurant payable"); Add(journal, clearing, 0m, intent.Amount, "Provider refund paid");
        await context.LedgerJournals.AddAsync(journal, ct);
    }

    public async Task StageSettlementPayoutJournalAsync(SettlementBatch batch, CancellationToken ct = default)
    {
        if (batch.NetAmount <= 0m || await ExistsForSourceAsync("settlement_payout", batch.Id, ct)) return;
        var wallet = await AccountAsync(batch.OrganizationId, "RESTAURANT_WALLET", "Restaurant wallet payable", "liability", batch.Currency, ct);
        var payable = await AccountAsync(null, "SETTLEMENT_PAYABLE", "Settlement payable clearing", "liability", batch.Currency, ct);
        var journal = new LedgerJournal
        {
            OrganizationId = batch.OrganizationId,
            Currency = batch.Currency,
            EntryType = "settlement_paid",
            SourceType = "settlement_payout",
            SourceId = batch.Id,
            Description = $"Settlement payout {batch.PeriodDate:yyyy-MM-dd} ({batch.PayoutReference})",
            PostedAt = batch.PaidAt ?? DateTime.UtcNow
        };
        Add(journal, wallet, batch.NetAmount, 0m, "Restaurant wallet settled");
        Add(journal, payable, 0m, batch.NetAmount, "Settlement payout payable");
        if (journal.Lines.Sum(l => l.Debit) != journal.Lines.Sum(l => l.Credit)) throw new InvalidOperationException("Ledger journal is not balanced.");
        await context.LedgerJournals.AddAsync(journal, ct);
    }

    public async Task<IReadOnlyList<LedgerAccountDto>> GetAccountsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var currency = await context.Organizations.AsNoTracking().Where(o => o.Id == organizationId).Select(o => o.CurrencyCode).SingleAsync(ct);
        return await context.LedgerAccounts.AsNoTracking()
            .Where(a => a.Currency == currency && (a.OrganizationId == organizationId || a.OrganizationId == null))
            .OrderBy(a => a.OrganizationId == null)
            .ThenBy(a => a.Code)
            .Select(a => new LedgerAccountDto(a.Id, a.OrganizationId, a.Code, a.Name, a.AccountType, a.Currency))
            .ToListAsync(ct);
    }

    public async Task<WalletBalanceDto> GetWalletAsync(Guid branchId, CancellationToken ct = default)
    {
        var branch = await context.Branches.Where(b => b.Id == branchId).Select(b => new { b.OrganizationId, b.Organization.CurrencyCode }).SingleAsync(ct);
        var available = await context.LedgerJournalLines.AsNoTracking().Where(l => l.Account.OrganizationId == branch.OrganizationId && l.Account.Code == "RESTAURANT_WALLET" && l.Account.Currency == branch.CurrencyCode).SumAsync(l => (decimal?)(l.Credit - l.Debit), ct) ?? 0m;
        var pending = await context.PaymentIntents.AsNoTracking().Where(p => p.BranchId == branchId && (p.Status == "pending" || p.Status == "created")).SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
        return new WalletBalanceDto(branch.OrganizationId, branch.CurrencyCode, available, pending, 0m, available);
    }

    public async Task<IReadOnlyList<LedgerJournalDto>> GetJournalAsync(Guid branchId, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var query = context.LedgerJournals.AsNoTracking().Where(j => j.BranchId == branchId);
        if (from.HasValue) query = query.Where(j => j.PostedAt >= from.Value);
        if (to.HasValue) query = query.Where(j => j.PostedAt <= to.Value);
        return await query.OrderByDescending(j => j.PostedAt).Take(500).Select(j => new LedgerJournalDto(j.Id, j.EntryType, j.SourceType, j.SourceId, j.Description, j.Currency, j.PostedAt, j.Lines.Sum(l => l.Debit), j.Lines.Sum(l => l.Credit))).ToListAsync(ct);
    }

    private async Task<LedgerAccount> AccountAsync(Guid? organizationId, string code, string name, string type, string currency, CancellationToken ct)
    {
        var existing = await context.LedgerAccounts.SingleOrDefaultAsync(a => a.OrganizationId == organizationId && a.Code == code && a.Currency == currency, ct);
        if (existing is not null) return existing;
        var account = new LedgerAccount { OrganizationId = organizationId, Code = code, Name = name, AccountType = type, Currency = currency, IsSystem = true };
        await context.LedgerAccounts.AddAsync(account, ct); return account;
    }
    private static void Add(LedgerJournal journal, LedgerAccount account, decimal debit, decimal credit, string memo) => journal.Lines.Add(new LedgerJournalLine { Account = account, Debit = debit, Credit = credit, Memo = memo });
}
