using FoodstoreApi.Core.Entities;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class PaymentIntentRepository(StoreDbContext context) : IPaymentIntentRepository
{
    private readonly StoreDbContext _context = context;

    public async Task<PaymentIntent?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PaymentIntents.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PaymentIntent?> GetByIdWithEventsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PaymentIntents
            .Include(p => p.Events)
            .Include(p => p.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<PaymentIntent?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return await _context.PaymentIntents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task<PaymentIntent?> GetByProviderReferenceAsync(string provider, string providerReference, CancellationToken ct = default)
    {
        return await _context.PaymentIntents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Provider == provider && p.ProviderReference == providerReference, ct);
    }

    public async Task<PaymentIntent?> GetActiveByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.PaymentIntents
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaymentIntent> AddAsync(PaymentIntent intent, CancellationToken ct = default)
    {
        await _context.PaymentIntents.AddAsync(intent, ct);
        await _context.SaveChangesAsync(ct);
        return intent;
    }

    public async Task UpdateAsync(PaymentIntent intent, CancellationToken ct = default)
    {
        _context.PaymentIntents.Update(intent);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<(IReadOnlyList<PaymentIntent> Items, int TotalCount)> SearchAsync(
        Guid? orderId, string? orderCode, string? phone, string? providerReference,
        string? status, DateTime? fromDate, DateTime? toDate, int page, int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.PaymentIntents
            .Include(p => p.Order)
            .AsNoTracking()
            .AsQueryable();

        if (orderId.HasValue)
            query = query.Where(p => p.OrderId == orderId.Value);
        if (!string.IsNullOrWhiteSpace(orderCode))
            query = query.Where(p => p.Order.OrderCode == orderCode);
        if (!string.IsNullOrWhiteSpace(phone))
            query = query.Where(p => p.CustomerPhone != null && p.CustomerPhone.Contains(phone));
        if (!string.IsNullOrWhiteSpace(providerReference))
            query = query.Where(p => p.ProviderReference == providerReference);
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);
        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt <= toDate.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IReadOnlyList<PaymentIntent>> GetStalePendingAsync(DateTime olderThanUtc, int max, CancellationToken ct = default)
    {
        return await _context.PaymentIntents
            .IgnoreQueryFilters()
            .Where(p => (p.Status == "pending" || p.Status == "created")
                        && p.ProviderReference != null
                        && p.CreatedAt <= olderThanUtc)
            .OrderBy(p => p.CreatedAt)
            .Take(max)
            .ToListAsync(ct);
    }

    public async Task PersistConfirmationAsync(
        PaymentIntent intent, PaymentEvent evt, Order order,
        Payment? payment, OrderStatusHistory? history, CancellationToken ct = default)
    {
        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        _context.PaymentIntents.Update(intent);

        if (evt.Id == Guid.Empty)
            await _context.PaymentEvents.AddAsync(evt, ct);
        else
            _context.PaymentEvents.Update(evt);

        _context.Orders.Update(order);

        if (payment is not null)
            await _context.Payments.AddAsync(payment, ct);

        if (history is not null)
            await _context.OrderStatusHistories.AddAsync(history, ct);

        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }
}
