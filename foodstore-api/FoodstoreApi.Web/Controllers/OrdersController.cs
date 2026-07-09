using FoodstoreApi.Core.Constants;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.DTOs.Order;
using FoodstoreApi.Web.Authorization;
using FoodstoreApi.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FoodstoreApi.Web.Hubs;
using FoodstoreApi.Web.ApiResponse;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize]
public class OrdersController(IOrderService service, IHubContext<AppHub> hubContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var orders = await service.GetAllAsync(cancellationToken);
        return ApiResult.Success(orders);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await service.GetByIdAsync(id, cancellationToken);
        if (order == null) return ApiResult.NotFound();
        return ApiResult.Success(order);
    }

    [HttpGet("{id:guid}/receipt")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetReceipt(Guid id, CancellationToken cancellationToken)
    {
        var receipt = await service.GetReceiptAsync(id, cancellationToken);
        return receipt is null ? ApiResult.NotFound() : ApiResult.Success(receipt);
    }

    [HttpPost]
    [RequirePermission("order.create")]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        return await ProcessCreateOrder(dto, cancellationToken);
    }

    [HttpPost("/api/pos/orders")]
    [RequirePermission("order.create")]
    public async Task<IActionResult> PosCreate([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        return await ProcessCreateOrder(dto, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("order.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = User.GetUserId();
            if (employeeId == Guid.Empty) return Unauthorized();

            var updated = await service.UpdateAsync(id, dto, employeeId, cancellationToken);
            await BroadcastAsync(updated.BranchId, "ReceiveOrderUpdate", updated, cancellationToken);
            await BroadcastAsync(updated.BranchId, "ReceiveTableUpdate", new { updated.BranchId }, cancellationToken);
            return ApiResult.Success(updated);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    [HttpPost("/api/pos/orders/{id:guid}/payment")]
    [RequirePermission("order.update")]
    public async Task<IActionResult> ProcessPayment(Guid id, [FromBody] ProcessPaymentDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = User.GetUserId();
            var order = await service.ProcessPaymentAsync(id, dto, employeeId != Guid.Empty ? employeeId : null, cancellationToken);

            await BroadcastAsync(order.BranchId, "ReceiveOrderUpdate", order, cancellationToken);
            await BroadcastAsync(order.BranchId, "ReceiveTableUpdate", new { order.BranchId }, cancellationToken);
            await BroadcastAsync(order.BranchId, "ReceiveNewOrder", order, cancellationToken);
            return ApiResult.Success(order);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    private async Task<IActionResult> ProcessCreateOrder(CreateOrderDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = User.GetUserId();
            if (employeeId == Guid.Empty) return Unauthorized();

            var created = await service.CreateAsync(dto, employeeId, cancellationToken);
            await BroadcastAsync(created.BranchId, "ReceiveOrderUpdate", created, cancellationToken);
            await BroadcastAsync(created.BranchId, "ReceiveTableUpdate", new { created.BranchId }, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/status")]
    [RequirePermission("order.update")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return await ProcessUpdateStatus(id, request, cancellationToken);
    }

    [HttpPut("/api/pos/orders/{id:guid}/status")]
    [RequirePermission("order.update")]
    public async Task<IActionResult> PosUpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        return await ProcessUpdateStatus(id, request, cancellationToken);
    }

    private async Task<IActionResult> ProcessUpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var employeeId = User.GetUserId();
            var result = await service.UpdateStatusAsync(id, request.Status, employeeId != Guid.Empty ? employeeId : null, request.PaymentMethod, request.PaymentAmount, cancellationToken);

            if (!result) return ApiResult.NotFound();
            var order = await service.GetByIdAsync(id, cancellationToken);
            await BroadcastAsync(order?.BranchId, "ReceiveOrderUpdate", new { Id = id, Status = request.Status }, cancellationToken);
            if (request.Status == OrderStatus.Paid)
                await BroadcastAsync(order?.BranchId, "ReceiveSourceUpdate", new { order?.BranchId }, cancellationToken);

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure(new ErrorDetail { Code = "INTERNAL_ERROR", Message = ex.Message }));
        }
    }

    [HttpGet("status/{status}")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken cancellationToken)
    {
        var orders = await service.GetByStatusAsync(status, cancellationToken);
        return ApiResult.Success(orders);
    }

    [HttpGet("date-range")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetByDateRange([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken cancellationToken)
    {
        var orders = await service.GetByDateRangeAsync(fromDate, toDate, cancellationToken);
        return ApiResult.Success(orders);
    }

    [HttpPut("{id:guid}/set-unpaid")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetUnpaid(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await service.GetByIdAsync(id, cancellationToken: cancellationToken);
            if (order == null) return ApiResult.NotFound();

            var employeeId = User.GetUserId();
            var result = await service.UpdateStatusAsync(id, OrderStatus.Pending, employeeId != Guid.Empty ? employeeId : null, null, null, cancellationToken);
            if (!result) return ApiResult.BadRequest("Failed to update order status");

            await BroadcastAsync(order.BranchId, "ReceiveOrderUpdate", new { Id = id, Status = OrderStatus.Pending }, cancellationToken);
            return ApiResult.Success(new { message = "Order set to unpaid successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.Failure(new ErrorDetail { Code = "INTERNAL_ERROR", Message = ex.Message }));
        }
    }

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await service.GetPagedAsync(page, pageSize, fromDate, toDate, cancellationToken);
        return ApiResult.Paged(items.ToList(), page, pageSize, totalCount);
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("order.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        if (!result) return ApiResult.NotFound();
        return NoContent();
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = null!;
        public string? PaymentMethod { get; set; }
        public decimal? PaymentAmount { get; set; }
    }


    private Task BroadcastAsync(Guid? branchId, string eventName, object payload, CancellationToken cancellationToken) =>
        branchId.HasValue
            ? hubContext.Clients.Group(TenantHubGroups.Branch(branchId.Value)).SendAsync(eventName, payload, cancellationToken)
            : Task.CompletedTask;
}
