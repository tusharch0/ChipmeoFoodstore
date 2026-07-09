using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Authorization;
using FoodstoreApi.Usecase.DTOs.Source;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.SignalR;
using FoodstoreApi.Web.Hubs;
using FoodstoreApi.Web.ApiResponse;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/admin/sources")]
[Authorize]
public class SourcesController(ISourceService service, IHubContext<AppHub> hubContext, ITenantContext tenantContext) : ControllerBase
{
    private readonly ISourceService _service = service;
    private readonly IHubContext<AppHub> _hubContext = hubContext;

    [HttpGet]
    [RequirePermission("source.view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var sources = await _service.GetAllAsync(cancellationToken);
        return ApiResult.Success(sources);
    }



    [HttpGet("{id:guid}")]
    [RequirePermission("source.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var source = await _service.GetByIdAsync(id, cancellationToken);
        if (source == null) return ApiResult.NotFound();
        return ApiResult.Success(source);
    }

    [HttpPost]
    [RequirePermission("source.create")]
    public async Task<IActionResult> Create([FromBody] CreateSourceDto dto, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(dto, cancellationToken);
        await NotifyBranchAsync(dto.BranchId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("source.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateSourceDto dto, CancellationToken cancellationToken)
    {
        var ok = await _service.UpdateAsync(id, dto, cancellationToken);
        if (!ok) return ApiResult.NotFound();
        await NotifyBranchAsync(dto.BranchId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("source.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _service.DeleteAsync(id, cancellationToken);
            if (!ok) return ApiResult.NotFound();
            await NotifyBranchAsync(null, cancellationToken);
            return NoContent();
        }
        catch (Exception)
        {
            return ApiResult.BadRequest("Không thể xóa nguồn này vì đang có đơn hàng liên kết.");
        }
    }

    private Task NotifyBranchAsync(Guid? requestedBranchId, CancellationToken cancellationToken)
    {
        var branchId = tenantContext.IsPlatformOperator ? requestedBranchId : tenantContext.BranchId;
        return branchId.HasValue
            ? _hubContext.Clients.Group(TenantHubGroups.Branch(branchId.Value)).SendAsync("ReceiveSourceUpdate", cancellationToken)
            : Task.CompletedTask;
    }
}




