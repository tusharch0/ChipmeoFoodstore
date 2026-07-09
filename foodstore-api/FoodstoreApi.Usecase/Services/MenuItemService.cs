using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.MenuItem;
using FoodstoreApi.Usecase.Utils;
using FoodstoreApi.Core.Constants;
using FoodstoreApi.Core.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodstoreApi.Usecase.Services;

public class MenuItemService : IMenuItemService
{
    private readonly IMenuItemRepository _repo;
    private readonly IDistributedCache _cache;
    private readonly IMediaService _mediaService;
    private readonly ITenantContext _tenantContext;

    public MenuItemService(IMenuItemRepository repo, IDistributedCache cache, IMediaService mediaService, ITenantContext tenantContext)
    {
        _repo = repo;
        _cache = cache;
        _mediaService = mediaService;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<MenuItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(AllKey, async () =>
        {
            var items = await _repo.GetAllAsync(cancellationToken);
            return items.Select(i => new MenuItemDto(
                i.Id,
                i.CategoryId,
                i.Name,
                i.Description,
                i.Price,
                i.ImageUrl,
                i.IsActive,
                i.CreatedAt,
                i.UpdatedAt,
                i.CreatedBy,
                i.UpdatedBy,
                i.Category?.Name,
                i.MenuItemAddons?.Select(ma => new Usecase.DTOs.Addon.AddonDto { Id = ma.Addon.Id, Name = ma.Addon.Name, Price = ma.Addon.Price, IsActive = ma.Addon.IsActive ?? false, CreatedAt = ma.Addon.CreatedAt, UpdatedAt = ma.Addon.UpdatedAt, CreatedBy = ma.Addon.CreatedBy, UpdatedBy = ma.Addon.UpdatedBy }).ToList()
            )).ToList();
        }, TimeSpan.FromMinutes(10), cancellationToken);
    }

    public async Task<MenuItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var i = await _repo.GetByIdAsync(id, cancellationToken);
        if (i == null) return null;
        return new MenuItemDto(
            i.Id, 
            i.CategoryId, 
            i.Name,
            i.Description,
            i.Price,
            i.ImageUrl,
            i.IsActive, 
            i.CreatedAt, 
            i.UpdatedAt,
            i.CreatedBy,
            i.UpdatedBy,
            i.Category?.Name,
            i.MenuItemAddons?.Select(ma => new Usecase.DTOs.Addon.AddonDto { Id = ma.Addon.Id, Name = ma.Addon.Name, Price = ma.Addon.Price, IsActive = ma.Addon.IsActive ?? false, CreatedAt = ma.Addon.CreatedAt, UpdatedAt = ma.Addon.UpdatedAt, CreatedBy = ma.Addon.CreatedBy, UpdatedBy = ma.Addon.UpdatedBy }).ToList()
        );
    }

    public async Task<MenuItemDto> CreateAsync(CreateMenuItemDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new MenuItem 
        { 
            CategoryId = dto.CategoryId, 
            BranchId = RequireBranchId(),
            Name = dto.Name, 
            Price = dto.Price,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            MenuItemAddons = dto.AddonIds?.Select(aid => new MenuItemAddon { AddonId = aid, IsActive = true }).ToList() ?? new List<MenuItemAddon>()
        };
        var created = await _repo.AddAsync(entity, cancellationToken);
        
        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            await _mediaService.LinkMediaToEntityAsync(dto.ImageUrl, "menu_item", created.Id);
        }

        await _cache.RemoveAsync(AllKey, cancellationToken);
        return new MenuItemDto(
            created.Id, 
            created.CategoryId, 
            created.Name,
            created.Description,
            created.Price,
            created.ImageUrl,
            created.IsActive, 
            created.CreatedAt, 
            created.UpdatedAt,
            created.CreatedBy,
            created.UpdatedBy,
            created.Category?.Name,
            new List<Usecase.DTOs.Addon.AddonDto>() 
        );
    }

    public async Task<bool> UpdateAsync(Guid id, CreateMenuItemDto dto, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;
        
        var oldImageUrl = existing.ImageUrl;

        existing.CategoryId = dto.CategoryId;
        existing.Name = dto.Name;
        existing.Price = dto.Price;
        existing.ImageUrl = dto.ImageUrl;
        existing.IsActive = dto.IsActive;

        if (dto.AddonIds != null)
        {
            existing.MenuItemAddons ??= new List<MenuItemAddon>();
            existing.MenuItemAddons.Clear();
            foreach (var aid in dto.AddonIds)
            {
                existing.MenuItemAddons.Add(new MenuItemAddon { AddonId = aid, IsActive = true });
            }
        }

        await _repo.UpdateAsync(existing, cancellationToken);
        
        if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl != oldImageUrl)
        {
            await _mediaService.LinkMediaToEntityAsync(dto.ImageUrl, "menu_item", id);
        }

        await _cache.RemoveAsync(AllKey, cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await _repo.GetByIdAsync(id, cancellationToken);
        if (existing == null) return false;
        
        await _repo.DeleteAsync(existing, cancellationToken);
        await _mediaService.DeleteMediaByEntityAsync("menu_item", id);
        await _cache.RemoveAsync(AllKey, cancellationToken);
        return true;
    }

    private string AllKey => $"{CacheKeys.MenuItems.All}:{_tenantContext.CacheScope}";
    private Guid RequireBranchId() => _tenantContext.BranchId ?? throw new InvalidOperationException("An active branch is required.");
}
