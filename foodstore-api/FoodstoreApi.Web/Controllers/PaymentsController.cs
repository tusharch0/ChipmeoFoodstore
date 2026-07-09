using FoodstoreApi.Usecase.DTOs.Payment;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodstoreApi.Web.Controllers;

/// <summary>
/// Customer/POS payment lifecycle: create an M-Pesa payment intent for an order and poll its status.
/// </summary>
[ApiController]
[Route("api/pos/payments")]
[Authorize]
public class PaymentsController(IPaymentService service) : ControllerBase
{
    [HttpPost]
    [RequirePermission("order.update")]
    public async Task<IActionResult> CreateIntent([FromBody] CreatePaymentIntentDto dto, CancellationToken ct)
    {
        try
        {
            var idempotencyKey = Request.Headers.TryGetValue("Idempotency-Key", out var key) && !string.IsNullOrWhiteSpace(key)
                ? key.ToString()
                : Guid.NewGuid().ToString("N");

            var intent = await service.CreateIntentAsync(dto, idempotencyKey, ct);
            return ApiResult.Success(intent);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var intent = await service.GetIntentAsync(id, ct);
        return intent is null ? ApiResult.NotFound() : ApiResult.Success(intent);
    }

    [HttpGet("by-order/{orderId:guid}")]
    [RequirePermission("order.view")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken ct)
    {
        var intent = await service.GetIntentByOrderAsync(orderId, ct);
        return intent is null ? ApiResult.NotFound() : ApiResult.Success(intent);
    }
}
