using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace FoodstoreApi.Web.Controllers;
[ApiController, Route("api/admin/finance"), Authorize]
public sealed class FinanceController(IFinanceService service, ITenantAccessService tenantAccess) : ControllerBase
{
    [HttpGet("accounts/{organizationId:guid}"), RequirePermission("finance.ledger.view")]
    public async Task<IActionResult> Accounts(Guid organizationId, CancellationToken ct) =>
        await tenantAccess.CanAccessOrganizationAsync(organizationId, ct) ? ApiResult.Success(await service.GetAccountsAsync(organizationId, ct)) : Forbid();
    [HttpGet("wallet/{branchId:guid}"), RequirePermission("finance.wallet.view")]
    public async Task<IActionResult> Wallet(Guid branchId, CancellationToken ct) =>
        await tenantAccess.CanAccessBranchAsync(branchId, ct) ? ApiResult.Success(await service.GetWalletAsync(branchId, ct)) : Forbid();
    [HttpGet("ledger/{branchId:guid}"), RequirePermission("finance.ledger.view")]
    public async Task<IActionResult> Ledger(Guid branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct) =>
        await tenantAccess.CanAccessBranchAsync(branchId, ct) ? ApiResult.Success(await service.GetJournalAsync(branchId, from, to, ct)) : Forbid();
}
