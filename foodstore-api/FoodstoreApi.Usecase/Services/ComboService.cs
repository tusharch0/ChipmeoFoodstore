using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Combo;
using FoodstoreApi.Usecase.Utils;
using FoodstoreApi.Core.Constants;
using FoodstoreApi.Core.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace FoodstoreApi.Usecase.Services;

public class ComboService : IComboService
{
    private readonly IComboRepository _repository;
    private readonly IMediaService _mediaService;
    private readonly IDistributedCache _cache;
    private readonly ITenantContext _tenantContext;

    public ComboService(IComboRepository repository, IMediaService mediaService, IDistributedCache cache, ITenantContext tenantContext)
    {
        _repository = repository;
        _mediaService = mediaService;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<ComboDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _cache.GetOrSetAsync(AllKey, async () =>
        {
            var combos = await _repository.GetAllAsync(cancellationToken);
            return combos.Select(MapToDto).ToList();
        }, TimeSpan.FromMinutes(30), cancellationToken);
    }

    public async Task<ComboDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var combo = await _repository.GetByIdAsync(id, cancellationToken);
        return combo == null ? null : MapToDto(combo);
    }

    public async Task<ComboDto> CreateAsync(CreateComboDto dto, CancellationToken cancellationToken = default)
    {
        var combo = new Combo
        {
            Name = dto.Name,
            ComboPrice = dto.ComboPrice,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            IsActive = dto.IsActive,
            BranchId = RequireBranchId(),

            ComboItems = dto.Items.Select(i => new ComboItem
            {
                MenuItemId = i.MenuItemId,
                Quantity = i.Quantity,

            }).ToList()
        };

        var created = await _repository.CreateAsync(combo, cancellationToken);
        
        if (!string.IsNullOrEmpty(dto.ImageUrl))
        {
            await _mediaService.LinkMediaToEntityAsync(dto.ImageUrl, "combo", created.Id);
        }

        await _cache.RemoveAsync(AllKey, cancellationToken);
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(Guid id, CreateComboDto dto, CancellationToken cancellationToken = default)
    {
        var combo = await _repository.GetByIdAsync(id, cancellationToken);
        if (combo == null) return false;

        var oldImageUrl = combo.ImageUrl;

        combo.Name = dto.Name;
        combo.ComboPrice = dto.ComboPrice;
        combo.Description = dto.Description;
        combo.ImageUrl = dto.ImageUrl;
        combo.IsActive = dto.IsActive;

        var newItems = dto.Items.Select(i => new ComboItem
        {
            ComboId = id,
            MenuItemId = i.MenuItemId,
            Quantity = i.Quantity,

        }).ToList();

        var result = await _repository.UpdateWithItemsAsync(combo, newItems, cancellationToken);
        
        if (result)
        {
            if (!string.IsNullOrEmpty(dto.ImageUrl) && dto.ImageUrl != oldImageUrl)
            {
                await _mediaService.LinkMediaToEntityAsync(dto.ImageUrl, "combo", id);
            }
            await _cache.RemoveAsync(AllKey, cancellationToken);
            await _cache.RemoveAsync(CacheKeys.Combos.ById(id), cancellationToken);
        }
        
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _repository.DeleteAsync(id, cancellationToken);
        if (result)
        {
            await _cache.RemoveAsync(AllKey, cancellationToken);
            await _cache.RemoveAsync(CacheKeys.Combos.ById(id), cancellationToken);
        }
        return result;
    }

    private static ComboDto MapToDto(Combo combo)
    {
        return new ComboDto
        {
            Id = combo.Id,
            Name = combo.Name,
            ComboPrice = combo.ComboPrice,
            Description = combo.Description,
            ImageUrl = combo.ImageUrl,
            IsActive = combo.IsActive ?? true,
            CreatedAt = combo.CreatedAt,
            UpdatedAt = combo.UpdatedAt,
            CreatedBy = combo.CreatedBy,
            UpdatedBy = combo.UpdatedBy,
            Items = combo.ComboItems?.Select(ci => new ComboItemDto
            {
                Id = ci.Id,
                MenuItemId = ci.MenuItemId,
                MenuItemName = ci.MenuItem?.Name ?? "",
                Quantity = ci.Quantity ?? 0,
                CreatedAt = ci.CreatedAt,
                UpdatedAt = ci.UpdatedAt
            }).ToList() ?? new List<ComboItemDto>()
        };
    }

    private string AllKey => $"{CacheKeys.Combos.All}:{_tenantContext.CacheScope}";
    private Guid RequireBranchId() => _tenantContext.BranchId ?? throw new InvalidOperationException("An active branch is required.");
}
