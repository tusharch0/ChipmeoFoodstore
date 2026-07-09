using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Source;
using FoodstoreApi.Usecase.Utils;
using FoodstoreApi.Core.Constants;
using FoodstoreApi.Core.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodstoreApi.Usecase.Services;

public class SourceService : ISourceService
{
    private readonly ISourceRepository _repository;
    private readonly IDistributedCache _cache;
    private readonly ITenantContext _tenantContext;

    public SourceService(ISourceRepository repository, IDistributedCache cache, ITenantContext tenantContext)
    {
        _repository = repository;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<SourceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(CacheKeys.Sources.AllForScope(_tenantContext.CacheScope), async () =>
        {
            var sources = await _repository.GetAllAsync(cancellationToken);
            return sources.Select(MapToDto).ToList();
        }, TimeSpan.FromMinutes(30), cancellationToken);
    }

    public async Task<SourceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var source = await _repository.GetByIdAsync(id, cancellationToken);
        return source == null ? null : MapToDto(source);
    }

    public async Task<SourceDto> CreateAsync(CreateSourceDto dto, CancellationToken cancellationToken = default)
    {
        var branchId = _tenantContext.IsPlatformOperator || _tenantContext.IsSystem
            ? dto.BranchId
            : _tenantContext.BranchId;
        if (!branchId.HasValue)
            throw new InvalidOperationException("An active branch is required to create an order source.");

        var source = new Source
        {
            Name = dto.Name,
            BranchId = branchId,
            IsActive = dto.IsActive,

        };

        var created = await _repository.CreateAsync(source, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Sources.AllForScope(_tenantContext.CacheScope), cancellationToken);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateSourceDto dto, CancellationToken cancellationToken = default)
    {
        var source = await _repository.GetByIdAsync(id, cancellationToken);
        if (source == null) return false;

        source.Name = dto.Name;
        if (_tenantContext.IsPlatformOperator || _tenantContext.IsSystem)
            source.BranchId = dto.BranchId ?? source.BranchId;
        source.IsActive = dto.IsActive;

        var result = await _repository.UpdateAsync(source, cancellationToken);
        if (result)
        {
            await _cache.RemoveAsync(CacheKeys.Sources.AllForScope(_tenantContext.CacheScope), cancellationToken);
            await _cache.RemoveAsync(CacheKeys.Sources.ById(id), cancellationToken);
        }
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _repository.DeleteAsync(id, cancellationToken);
        if (result)
        {
            await _cache.RemoveAsync(CacheKeys.Sources.AllForScope(_tenantContext.CacheScope), cancellationToken);
            await _cache.RemoveAsync(CacheKeys.Sources.ById(id), cancellationToken);
        }
        return result;
    }

    private static SourceDto MapToDto(Source source)
    {
        return new SourceDto
        {
            Id = source.Id,
            Name = source.Name,
            IsActive = source.IsActive ?? true,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt,
            CreatedBy = source.CreatedBy,
            UpdatedBy = source.UpdatedBy
        };
    }
}
