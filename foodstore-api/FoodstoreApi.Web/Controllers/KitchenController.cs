using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FoodstoreApi.Web.Hubs;
using FoodstoreApi.Web.ApiResponse;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/kitchen")]
[Authorize]
public class KitchenController(IOrderService orderService, IHubContext<AppHub> hubContext, ITenantContext tenantContext) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;
    private readonly IHubContext<AppHub> _hubContext = hubContext;

    /// <summary>
    /// Get all paid orders for kitchen (status: paid or preparing)
    /// </summary>
    [HttpGet("orders")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetPaidOrdersForKitchenAsync(cancellationToken);
        return ApiResult.Success(orders);
    }

    /// <summary>
    /// Get only orders that are ready to be prepared (status: paid)
    /// </summary>
    [HttpGet("orders/pending")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetPendingOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetByStatusAsync("paid", cancellationToken);
        return ApiResult.Success(orders);
    }

    /// <summary>
    /// Get orders currently being prepared (status: preparing)
    /// </summary>
    [HttpGet("orders/preparing")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetPreparingOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetByStatusAsync("preparing", cancellationToken);
        return ApiResult.Success(orders);
    }

    /// <summary>
    /// Start preparing an order (paid → preparing)
    /// </summary>
    [HttpPut("orders/{id:guid}/start")]
    [RequirePermission("order.update")]
    public async Task<IActionResult> StartPreparing(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _orderService.UpdateKitchenStatusAsync(id, "preparing", cancellationToken);
            if (!success) return ApiResult.NotFound();

            // Notify all clients about status change
            await NotifyBranchAsync(new { Id = id, Status = "preparing" }, cancellationToken);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Complete an order (preparing → completed)
    /// </summary>
    [HttpPut("orders/{id:guid}/complete")]
    [RequirePermission("order.update")]
    public async Task<IActionResult> CompleteOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var success = await _orderService.UpdateKitchenStatusAsync(id, "served", cancellationToken);
            if (!success) return ApiResult.NotFound();

            // Notify all clients about completion
            await NotifyBranchAsync(new { Id = id, Status = "served" }, cancellationToken);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    private Task NotifyBranchAsync(object payload, CancellationToken cancellationToken) =>
        tenantContext.BranchId.HasValue
            ? _hubContext.Clients.Group(TenantHubGroups.Branch(tenantContext.BranchId.Value)).SendAsync("ReceiveOrderUpdate", payload, cancellationToken)
            : Task.CompletedTask;
}




