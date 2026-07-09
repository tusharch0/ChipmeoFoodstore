using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Authorization;
using FoodstoreApi.Usecase.DTOs.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodstoreApi.Web.ApiResponse;
using System.Security.Claims;
using System.Text;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    private readonly ITenantAccessService _tenantAccess;

    public ReportsController(IReportService service, ITenantAccessService tenantAccess)
    {
        _service = service;
        _tenantAccess = tenantAccess;
    }

    [HttpGet("dashboard-stats")]
    [RequirePermission("analytics.view")]
    public async Task<IActionResult> GetDashboardStats(CancellationToken cancellationToken)
    {
        var stats = await _service.GetDashboardStatsAsync(cancellationToken);
        return ApiResult.Success(stats);
    }

    [HttpGet("financial/{organizationId:guid}")]
    [RequirePermission("analytics.view")]
    public async Task<IActionResult> GetFinancialReport(Guid organizationId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var authorized = branchId.HasValue
            ? await _tenantAccess.CanAccessBranchAsync(branchId.Value, cancellationToken)
            : await _tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken);
        if (!authorized)
            return Forbid();

        try
        {
            return ApiResult.Success(await _service.GetFinancialReportAsync(new FinancialReportRequest(
                organizationId, fromDate, toDate, branchId, Actor(), false), cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return ApiResult.BadRequest(exception.Message);
        }
    }

    [HttpGet("financial/{organizationId:guid}/export")]
    [RequirePermission("analytics.export")]
    public async Task<IActionResult> ExportFinancialReport(Guid organizationId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var authorized = branchId.HasValue
            ? await _tenantAccess.CanAccessBranchAsync(branchId.Value, cancellationToken)
            : await _tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken);
        if (!authorized)
            return Forbid();

        try
        {
            var report = await _service.GetFinancialReportAsync(new FinancialReportRequest(
                organizationId, fromDate, toDate, branchId, Actor(), true), cancellationToken);
            var csv = new StringBuilder();
            csv.AppendLine("Branch,Orders,Successful Payments,Payment Conversion %,Gross Sales,Refunds,Net Sales,Wallet Position");
            foreach (var branch in report.Branches)
                csv.AppendLine($"{Escape(branch.BranchName)},{branch.Orders},{branch.SuccessfulPayments},{branch.PaymentConversionRate},{branch.GrossSales},{branch.Refunds},{branch.NetSales},{branch.WalletPosition}");
            csv.AppendLine($"TOTAL,{report.Totals.Orders},{report.Totals.SuccessfulPayments},{report.Totals.PaymentConversionRate},{report.Totals.GrossSales},{report.Totals.Refunds},{report.Totals.NetSales},{report.Totals.WalletPosition}");
            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"financial-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.csv");
        }
        catch (InvalidOperationException exception)
        {
            return ApiResult.BadRequest(exception.Message);
        }
    }

    private Guid Actor() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id
        : throw new InvalidOperationException("Authenticated actor is required.");

    private static string Escape(string value) => value.Contains(',', StringComparison.Ordinal) ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}




