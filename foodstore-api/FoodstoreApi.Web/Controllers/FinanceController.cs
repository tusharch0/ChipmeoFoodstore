using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
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
    [HttpGet("ledger/{branchId:guid}/export"), RequirePermission("finance.ledger.view")]
    public async Task<IActionResult> ExportLedger(Guid branchId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (!await tenantAccess.CanAccessBranchAsync(branchId, ct)) return Forbid();
        var entries = await service.GetJournalAsync(branchId, from, to, ct);
        var csv = new StringBuilder("Journal ID,Entry Type,Source Type,Internal Source ID,Provider Reference,Description,Currency,Posted At,Debits,Credits\n");
        foreach (var entry in entries)
            csv.AppendLine($"{entry.Id},{Escape(entry.EntryType)},{Escape(entry.SourceType)},{entry.SourceId},{Escape(entry.ProviderReference)},{Escape(entry.Description)},{entry.Currency},{entry.PostedAt:O},{entry.Debits},{entry.Credits}");
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"wallet-statement-{branchId:N}-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
    private static string Escape(string? value) => string.IsNullOrEmpty(value) ? string.Empty : $"\"{value.Replace("\"", "\"\"")}\"";
}
