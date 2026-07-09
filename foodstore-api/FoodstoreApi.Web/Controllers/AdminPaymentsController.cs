using FoodstoreApi.Usecase.DTOs.Payment;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodstoreApi.Web.Controllers;

/// <summary>
/// Finance support console: search payments and inspect their webhook/event history and lifecycle.
/// </summary>
[ApiController]
[Route("api/admin/payments")]
[Authorize]
public class AdminPaymentsController(IPaymentService service) : ControllerBase
{
    [HttpGet]
    [RequirePermission("payment.transactions.view")]
    public async Task<IActionResult> Search(
        [FromQuery] Guid? orderId,
        [FromQuery] string? orderCode,
        [FromQuery] string? phone,
        [FromQuery] string? providerReference,
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new PaymentSearchFilter(orderId, orderCode, phone, providerReference, status, fromDate, toDate, page, pageSize);
        var (items, total) = await service.SearchAsync(filter, ct);
        return ApiResult.Paged(items.ToList(), page, pageSize, total);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("payment.transactions.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var detail = await service.GetDetailAsync(id, ct);
        return detail is null ? ApiResult.NotFound() : ApiResult.Success(detail);
    }

    [HttpPost("{id:guid}/refund")]
    [RequirePermission("payment.refund")]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentDto dto, CancellationToken ct)
    {
        try
        {
            var intent = await service.RefundAsync(id, dto.Reason, ct);
            return ApiResult.Success(intent);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }
}
