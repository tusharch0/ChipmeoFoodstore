using FoodstoreApi.Core.Constants;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Core.Entities;
using FoodstoreApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class OrderRepository(StoreDbContext context) : IOrderRepository
{
    private readonly StoreDbContext _context = context;

    public async Task<IEnumerable<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AsNoTracking()
            .Include(o => o.Source)
            .Include(o => o.Employee)
            .Include(o => o.Discount)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemAddons)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Source)
            .Include(o => o.Employee)
            .Include(o => o.Discount)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemAddons)
            .Include(o => o.OrderStatusHistories)
                .ThenInclude(h => h.ChangedByNavigation)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task<bool> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Update(order);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (order == null) return false;

        _context.Orders.Remove(order);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AsNoTracking()
            .Include(o => o.Source)
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemAddons)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetBySourceIdAsync(Guid sourceId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .Where(o => o.SourceId == sourceId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
    {
        return await _context.Orders.AsNoTracking()
            .Include(o => o.Source)
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
            .Where(o => o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Order> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders.AsNoTracking();

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAt >= fromDate.Value);
        
        if (toDate.HasValue)
            query = query.Where(o => o.CreatedAt <= toDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(o => o.Source)
            .Include(o => o.Employee)
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Include(o => o.OrderStatusHistories)
                .ThenInclude(h => h.ChangedByNavigation)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Order?> GetLastOrderByCodePrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Where(o => o.OrderCode.StartsWith(prefix))
            .OrderByDescending(o => o.OrderCode)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddHistoryAsync(OrderStatusHistory history, CancellationToken cancellationToken = default)
    {
        await _context.OrderStatusHistories.AddAsync(history, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> CountActiveOrdersBySourceIdAsync(Guid sourceId, Guid? excludeOrderId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Where(o => o.SourceId == sourceId && 
                        o.Status != "paid" && 
                        o.Status != "cancelled" &&
                        o.Status != "completed");

        if (excludeOrderId.HasValue)
        {
            query = query.Where(o => o.Id != excludeOrderId.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetPaidOrdersForKitchenAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Source)
            .Include(o => o.Employee)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemAddons)
            .Where(o => o.Status == OrderStatus.Paid || o.Status == OrderStatus.Preparing || (o.Status == OrderStatus.Served && o.UpdatedAt >= DateTime.UtcNow.Date))
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
