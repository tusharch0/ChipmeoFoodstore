using System.Security.Claims;
using FoodstoreApi.Usecase.DTOs.Finance;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FoodstoreApi.Web.Controllers;
[ApiController, Route("api/admin/finance/operations"), Authorize]
public sealed class FinanceOperationsController(IFinanceOperationsService service, ITenantAccessService tenantAccess, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("exceptions/unmatched"), RequirePermission("finance.ledger.view")]
    public async Task<IActionResult> UnmatchedPayments(CancellationToken ct) => tenantContext.IsPlatformOperator || tenantContext.IsSystem ? ApiResult.Success(await service.UnmatchedPaymentsAsync(ct)) : Forbid();
    [HttpGet("exceptions/{organizationId:guid}"), RequirePermission("finance.ledger.view")]
    public async Task<IActionResult> Exceptions(Guid organizationId, CancellationToken ct) => await tenantAccess.CanAccessOrganizationAsync(organizationId, ct) ? ApiResult.Success(await service.ExceptionsAsync(organizationId, ct)) : Forbid();
    [HttpGet("adjustments/{organizationId:guid}"), RequirePermission("finance.adjust.request")]
    public async Task<IActionResult> Adjustments(Guid organizationId, CancellationToken ct) => await tenantAccess.CanAccessOrganizationAsync(organizationId, ct) ? ApiResult.Success(await service.AdjustmentsAsync(organizationId, ct)) : Forbid();
    [HttpPost("adjustments"), RequirePermission("finance.adjust.request")]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateAdjustmentRequest request, CancellationToken ct) { try { if (!await tenantAccess.CanAccessOrganizationAsync(request.OrganizationId, ct)) return Forbid(); return ApiResult.Success(await service.RequestAdjustmentAsync(request, Actor(), ct)); } catch (InvalidOperationException e) { return ApiResult.BadRequest(e.Message); } }
    [HttpPost("adjustments/{id:guid}/approve"), RequirePermission("finance.adjust.approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct) { try { var adjustment = await service.GetAdjustmentAsync(id, ct); if (adjustment is null) return ApiResult.NotFound(); if (!await tenantAccess.CanAccessOrganizationAsync(adjustment.OrganizationId, ct)) return Forbid(); return ApiResult.Success(await service.ApproveAdjustmentAsync(id, Actor(), ct)); } catch (InvalidOperationException e) { return ApiResult.BadRequest(e.Message); } }
    [HttpGet("reconciliation/{organizationId:guid}"), RequirePermission("finance.ledger.view")]
    public async Task<IActionResult> Cases(Guid organizationId, CancellationToken ct) => await tenantAccess.CanAccessOrganizationAsync(organizationId, ct) ? ApiResult.Success(await service.CasesAsync(organizationId, ct)) : Forbid();
    [HttpPost("reconciliation"), RequirePermission("finance.adjust.request")]
    public async Task<IActionResult> CreateCase([FromBody] CreateReconciliationRequest request, CancellationToken ct) => await tenantAccess.CanAccessOrganizationAsync(request.OrganizationId, ct) ? ApiResult.Success(await service.CreateCaseAsync(request, Actor(), ct)) : Forbid();
    [HttpPost("reconciliation/{id:guid}/resolve"), RequirePermission("finance.adjust.approve")]
    public async Task<IActionResult> Resolve(Guid id, [FromBody] ResolveReconciliationRequest request, CancellationToken ct) { try { var item = await service.GetCaseAsync(id, ct); if (item is null) return ApiResult.NotFound(); if (!await tenantAccess.CanAccessOrganizationAsync(item.OrganizationId, ct)) return Forbid(); return ApiResult.Success(await service.ResolveCaseAsync(id, request, Actor(), ct)); } catch (InvalidOperationException e) { return ApiResult.BadRequest(e.Message); } }
    private Guid Actor() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new InvalidOperationException("Authenticated actor is required.");
}
