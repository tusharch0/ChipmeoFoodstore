using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Core.Entities;
using FoodstoreApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class SourceRepository : ISourceRepository
{
    private readonly StoreDbContext _context;

    public SourceRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Source>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Sources.ToListAsync(cancellationToken);
    }

    public async Task<Source?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sources.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Source> CreateAsync(Source source, CancellationToken cancellationToken = default)
    {
        _context.Sources.Add(source);
        await _context.SaveChangesAsync(cancellationToken);
        return source;
    }

    public async Task<bool> UpdateAsync(Source source, CancellationToken cancellationToken = default)
    {
        _context.Sources.Update(source);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _context.Sources.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (source == null) return false;
        
        _context.Sources.Remove(source);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
