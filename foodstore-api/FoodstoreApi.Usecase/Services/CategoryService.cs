using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Category;
using FoodstoreApi.Usecase.Utils;
using FoodstoreApi.Core.Constants;
using FoodstoreApi.Core.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodstoreApi.Usecase.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly IMediaService _mediaService;
    private readonly IDistributedCache _cache;
    private readonly ITenantContext _tenantContext;

    public CategoryService(ICategoryRepository repo, IMediaService mediaService, IDistributedCache cache, ITenantContext tenantContext)
    {
        _repo = repo;
        _mediaService = mediaService;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(AllKey, async () =>
        {
            var categories = await _repo.GetAllAsync(cancellationToken);
            return categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.ImageUrl, c.IsActive, c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy)).ToList();
        }, TimeSpan.FromMinutes(30), cancellationToken);
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var c = await _repo.GetByIdAsync(id, cancellationToken);
        if (c == null) return null;
        return new CategoryDto(c.Id, c.Name, c.Description, c.ImageUrl, c.IsActive, c.CreatedAt, c.UpdatedAt, c.CreatedBy, c.UpdatedBy);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new Category { Name = dto.Name, Description = dto.Description, ImageUrl = dto.ImageUrl, IsActive = dto.IsActive, BranchId = RequireBranchId() };
        var created = await _repo.AddAsync(entity, cancellationToken);
        
        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            await _mediaService.LinkMediaToEntityAsync(dto.ImageUrl, "category", created.Id);
        }

        await _cache.RemoveAsync(AllKey, cancellationToken);
        await _cache.RemoveAsync(MenuItemsKey, cancellationToken);
        return new CategoryDto(created.Id, created.Name, created.Description, created.ImageUrl, created.IsActive, created.CreatedAt, created.UpdatedAt, created.CreatedBy, created.UpdatedBy);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;
        
        var oldImageUrl = existing.ImageUrl;

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.ImageUrl = dto.ImageUrl;
        existing.IsActive = dto.IsActive;
        
        await _repo.UpdateAsync(existing, cancellationToken);

        if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl != oldImageUrl)
        {
            await _mediaService.LinkMediaToEntityAsync(dto.ImageUrl, "category", id);
        }

        await _cache.RemoveAsync(AllKey, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Categories.ById(id), cancellationToken);
        await _cache.RemoveAsync(MenuItemsKey, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;
        
        await _repo.DeleteAsync(existing, cancellationToken);
        await _mediaService.DeleteMediaByEntityAsync("category", id);
        
        await _cache.RemoveAsync(AllKey, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.Categories.ById(id), cancellationToken);
        await _cache.RemoveAsync(MenuItemsKey, cancellationToken);
        return true;
    }

    private string AllKey => $"{CacheKeys.Categories.All}:{_tenantContext.CacheScope}";
    private string MenuItemsKey => $"{CacheKeys.MenuItems.All}:{_tenantContext.CacheScope}";
    private Guid RequireBranchId() => _tenantContext.BranchId ?? throw new InvalidOperationException("An active branch is required.");
}
