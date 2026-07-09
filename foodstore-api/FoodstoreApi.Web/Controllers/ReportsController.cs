using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Authorization;
using FoodstoreApi.Usecase.DTOs.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodstoreApi.Web.ApiResponse;
using System.Security.Claims;
using System.Text;
using System.Globalization;

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

    [HttpGet("financial/{organizationId:guid}/export.pdf")]
    [RequirePermission("analytics.export")]
    public async Task<IActionResult> ExportFinancialReportPdf(Guid organizationId, [FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        var authorized = branchId.HasValue ? await _tenantAccess.CanAccessBranchAsync(branchId.Value, cancellationToken) : await _tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken);
        if (!authorized) return Forbid();
        try
        {
            var report = await _service.GetFinancialReportAsync(new FinancialReportRequest(organizationId, fromDate, toDate, branchId, Actor(), true), cancellationToken);
            var lines = new List<string> { "Financial report", $"Period: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd} ({report.TimeZone})", $"Currency: {report.Currency}", "", "Branch | Orders | Payments | Gross | Refunds | Net | Wallet" };
            lines.AddRange(report.Branches.Select(branch => $"{branch.BranchName} | {branch.Orders} | {branch.SuccessfulPayments} | {branch.GrossSales:0.00} | {branch.Refunds:0.00} | {branch.NetSales:0.00} | {branch.WalletPosition:0.00}"));
            lines.Add("");
            lines.Add($"TOTAL | {report.Totals.Orders} | {report.Totals.SuccessfulPayments} | {report.Totals.GrossSales:0.00} | {report.Totals.Refunds:0.00} | {report.Totals.NetSales:0.00} | {report.Totals.WalletPosition:0.00}");
            return File(BuildPdf(lines), "application/pdf", $"financial-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.pdf");
        }
        catch (InvalidOperationException exception) { return ApiResult.BadRequest(exception.Message); }
    }

    private static byte[] BuildPdf(IReadOnlyList<string> lines)
    {
        static string PdfText(string value) => new string(value.Select(character => character is >= ' ' and <= '~' ? character : '?').ToArray()).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var content = new StringBuilder("BT /F1 9 Tf 36 806 Td 12 TL ");
        foreach (var line in lines.Take(62)) content.Append('(').Append(PdfText(line)).Append(") Tj T* ");
        content.Append("ET");
        var objects = new[] { "<< /Type /Catalog /Pages 2 0 R >>", "<< /Type /Pages /Kids [3 0 R] /Count 1 >>", "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>", "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>", $"<< /Length {Encoding.ASCII.GetByteCount(content.ToString())} >>\nstream\n{content}\nendstream" };
        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new List<int>();
        for (var index = 0; index < objects.Length; index++) { offsets.Add(Encoding.ASCII.GetByteCount(pdf.ToString())); pdf.Append(index + 1).Append(" 0 obj\n").Append(objects[index]).Append("\nendobj\n"); }
        var xref = Encoding.ASCII.GetByteCount(pdf.ToString());
        pdf.Append("xref\n0 ").Append(objects.Length + 1).Append("\n0000000000 65535 f \n");
        foreach (var offset in offsets) pdf.Append(offset.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        pdf.Append("trailer << /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF");
        return Encoding.ASCII.GetBytes(pdf.ToString());
    }
}




