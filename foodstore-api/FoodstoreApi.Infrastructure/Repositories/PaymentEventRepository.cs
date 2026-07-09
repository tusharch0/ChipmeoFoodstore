using FoodstoreApi.Core.Entities;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class PaymentEventRepository(StoreDbContext context) : IPaymentEventRepository
{
    private readonly StoreDbContext _context = context;

    public async Task<bool> ExistsByProviderEventIdAsync(string provider, string providerEventId, CancellationToken ct = default)
    {
        return await _context.PaymentEvents
            .AnyAsync(e => e.Provider == provider && e.ProviderEventId == providerEventId, ct);
    }

    public async Task<PaymentEvent> AddAsync(PaymentEvent evt, CancellationToken ct = default)
    {
        await _context.PaymentEvents.AddAsync(evt, ct);
        await _context.SaveChangesAsync(ct);
        return evt;
    }

    public async Task UpdateAsync(PaymentEvent evt, CancellationToken ct = default)
    {
        _context.PaymentEvents.Update(evt);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentEvent>> GetUnprocessedAsync(int max, CancellationToken ct = default)
    {
        return await _context.PaymentEvents
            .Where(e => e.ProcessingOutcome == null)
            .OrderBy(e => e.ReceivedAt)
            .Take(max)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PaymentEvent>> GetByIntentIdAsync(Guid intentId, CancellationToken ct = default)
    {
        return await _context.PaymentEvents
            .AsNoTracking()
            .Where(e => e.PaymentIntentId == intentId)
            .OrderByDescending(e => e.ReceivedAt)
            .ToListAsync(ct);
    }
}
