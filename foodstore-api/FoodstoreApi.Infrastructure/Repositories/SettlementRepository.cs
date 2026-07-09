using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace FoodstoreApi.Infrastructure.Repositories;
public sealed class SettlementRepository(StoreDbContext context) : ISettlementRepository
{
    public Task<SettlementBatch?> GetAsync(Guid id, CancellationToken ct = default) => context.SettlementBatches.Include(b => b.Lines).SingleOrDefaultAsync(b => b.Id == id, ct);
    public async Task<IReadOnlyList<SettlementBatch>> GetAllAsync(Guid organizationId, CancellationToken ct = default) => await context.SettlementBatches.AsNoTracking().Where(b => b.OrganizationId == organizationId).OrderByDescending(b => b.PeriodDate).ToListAsync(ct);
    public async Task<SettlementBatch> GenerateAsync(Guid organizationId, DateOnly date, string currency, Guid actorId, CancellationToken ct = default)
    {
        var key = $"{organizationId:N}:{date:yyyyMMdd}:{currency}"; var existing = await context.SettlementBatches.Include(b => b.Lines).SingleOrDefaultAsync(b => b.IdempotencyKey == key, ct); if (existing is not null) return existing;
        var start = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc); var end = start.AddDays(1);
        var policy = await context.FinancePolicies.SingleOrDefaultAsync(p => p.OrganizationId == organizationId && p.IsActive, ct);
        var intents = await context.PaymentIntents.IgnoreQueryFilters().Where(p => p.Order.Branch!.OrganizationId == organizationId && p.Currency == currency && p.Status == "succeeded" && p.UpdatedAt >= start && p.UpdatedAt < end && !context.SettlementLines.Any(l => l.PaymentIntentId == p.Id)).ToListAsync(ct);
        decimal Calc(string mode, decimal value, decimal amount) => mode == "percentage" ? Math.Round(amount * value / 100m, 2) : value;
        var batch = new SettlementBatch { OrganizationId = organizationId, Currency = currency, PeriodDate = date, IdempotencyKey = key, CreatedBy = actorId };
        foreach (var p in intents) { var commission = policy is null ? 0m : Calc(policy.CommissionMode, policy.CommissionValue, p.Amount); var fee = policy is null ? 0m : Calc(policy.ProviderFeeMode, policy.ProviderFeeValue, p.Amount); var net = p.Amount - commission - (policy?.FeeBearer == "restaurant" ? fee : 0m); batch.Lines.Add(new SettlementLine { PaymentIntentId = p.Id, GrossAmount = p.Amount, CommissionAmount = commission, ProviderFeeAmount = fee, NetAmount = net }); }
        batch.GrossSales = batch.Lines.Sum(l => l.GrossAmount); batch.Commissions = batch.Lines.Sum(l => l.CommissionAmount); batch.ProviderFees = batch.Lines.Sum(l => l.ProviderFeeAmount); batch.NetAmount = batch.Lines.Sum(l => l.NetAmount);
        if (policy is not null && batch.NetAmount < policy.MinimumPayoutThreshold) batch.Holds = batch.NetAmount;
        await context.SettlementBatches.AddAsync(batch, ct); await context.SaveChangesAsync(ct); return batch;
    }
    public async Task SaveAsync(SettlementBatch batch, CancellationToken ct = default) { context.SettlementBatches.Update(batch); await context.SaveChangesAsync(ct); }
}
