using System;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Authorization;
using FoodstoreApi.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodstoreApi.Web.ApiResponse;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/admin/payment-settings")]
[Authorize]
public class PaymentSettingsController : ControllerBase
{
    private readonly IPaymentSettingService _service;

    public PaymentSettingsController(IPaymentSettingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var setting = await _service.GetAsync(cancellationToken);
        return ApiResult.Success(setting ?? new PaymentSetting());
    }

    /// <summary>
    /// Get all payment settings (for admin UI)
    /// </summary>
    [HttpGet("all")]
    [RequirePermission("payment.view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var settings = await _service.GetAllAsync(cancellationToken);
        return ApiResult.Success(settings);
    }

    /// <summary>
    /// Get payment setting by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [RequirePermission("payment.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var setting = await _service.GetByIdAsync(id, cancellationToken);
        if (setting == null)
        {
            return ApiResult.NotFound($"Payment setting with ID {id} not found");
        }
        return ApiResult.Success(setting);
    }

    /// <summary>
    /// Create or update payment setting
    /// </summary>
    [HttpPost]
    [RequirePermission("payment.update")]
    public async Task<IActionResult> Save([FromBody] PaymentSetting setting, CancellationToken cancellationToken)
    {
        try
        {
            var saved = await _service.SaveAsync(setting, cancellationToken);
            return ApiResult.Success(saved);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update specific payment setting
    /// </summary>
    [HttpPut("{id:guid}")]
    [RequirePermission("payment.update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PaymentSetting setting, CancellationToken cancellationToken)
    {
        setting.Id = id;
        try
        {
            var saved = await _service.SaveAsync(setting, cancellationToken);
            return ApiResult.Success(saved);
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Set payment setting as default
    /// </summary>
    [HttpPut("{id:guid}/set-default")]
    [RequirePermission("payment.update")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.SetDefaultAsync(id, cancellationToken);
            return ApiResult.Success(new { message = "Payment setting set as default successfully" });
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Delete payment setting (cannot delete default)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [RequirePermission("payment.delete")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return ApiResult.Success(new { message = "Payment setting deleted successfully" });
        }
        catch (Exception ex)
        {
            return ApiResult.BadRequest(ex.Message);
        }
    }
}




