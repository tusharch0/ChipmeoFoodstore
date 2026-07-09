using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Authorization;
using FoodstoreApi.Usecase.DTOs.MenuItem;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.SignalR;
using FoodstoreApi.Web.Hubs;
using FoodstoreApi.Web.ApiResponse;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/admin/menu-items")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemService _service;
    private readonly IHubContext<AppHub> _hubContext;
    private readonly ITenantContext _tenantContext;

    public MenuItemsController(IMenuItemService service, IHubContext<AppHub> hubContext, ITenantContext tenantContext)
    {
        _service = service;
        _hubContext = hubContext;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequirePermission("menu.view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return ApiResult.Success(items);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("menu.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null) return ApiResult.NotFound();
        return ApiResult.Success(item);
    }

    [HttpPost]
    [RequirePermission("menu.create")]
    public async Task<IActionResult> Create(CreateMenuItemDto dto, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(dto, cancellationToken);
        await BroadcastAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("menu.update")]
    public async Task<IActionResult> Update(Guid id, CreateMenuItemDto dto, CancellationToken cancellationToken)
    {
        var ok = await _service.UpdateAsync(id, dto, cancellationToken);
        if (!ok) return ApiResult.NotFound();
        await BroadcastAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("menu.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _service.DeleteAsync(id, cancellationToken);
            if (!ok) return ApiResult.NotFound();
            await BroadcastAsync(cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return ApiResult.BadRequest("Không thể xóa món này vì đã có trong đơn hàng hoặc combo.");
        }
    }

    private Task BroadcastAsync(CancellationToken cancellationToken) => _tenantContext.BranchId.HasValue
        ? _hubContext.Clients.Group(TenantHubGroups.Branch(_tenantContext.BranchId.Value)).SendAsync("ReceiveMenuUpdate", cancellationToken)
        : Task.CompletedTask;

    // POS endpoints for read-only menu (public)
    [HttpGet("/api/pos/menu-items")]
    public async Task<IActionResult> PosGetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return ApiResult.Success(items);
    }

    [HttpGet("/api/pos/menu-items/{id:guid}")]
    public async Task<IActionResult> PosGetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        if (item == null) return ApiResult.NotFound();
        return ApiResult.Success(item);
    }
}
