using System.Security.Claims;
using System.Text;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FoodstoreApi.Web.Hubs;
namespace FoodstoreApi.Web.Controllers;
[ApiController, Route("api/admin/settlements"), Authorize]
public sealed class SettlementsController(ISettlementService service, IHubContext<AppHub> hub, ITenantAccessService tenantAccess) : ControllerBase
{
    [HttpGet("batch/{id:guid}"), RequirePermission("settlement.view")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) { var batch = await service.GetAsync(id, ct); return batch is null ? ApiResult.NotFound() : !await tenantAccess.CanAccessOrganizationAsync(batch.OrganizationId, ct) ? Forbid() : ApiResult.Success(batch); }
    [HttpGet("batch/{id:guid}/export"), RequirePermission("settlement.view")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct)
    {
        var batch = await service.GetAsync(id, ct);
        if (batch is null) return ApiResult.NotFound();
        if (!await tenantAccess.CanAccessOrganizationAsync(batch.OrganizationId, ct)) return Forbid();
        var csv = new StringBuilder();
        csv.AppendLine("Settlement ID,Organization ID,Period Date,Status,Currency,Gross Sales,Commissions,Provider Fees,Adjustments,Holds,Net Amount,Payout Reference");
        csv.AppendLine($"{batch.Id},{batch.OrganizationId},{batch.PeriodDate:yyyy-MM-dd},{batch.Status},{batch.Currency},{batch.GrossSales},{batch.Commissions},{batch.ProviderFees},{batch.Adjustments},{batch.Holds},{batch.NetAmount},{Escape(batch.PayoutReference)}");
        csv.AppendLine();
        csv.AppendLine("Payment Intent ID,Gross Amount,Commission Amount,Provider Fee Amount,Net Amount");
        foreach (var line in batch.Lines.OrderBy(l => l.CreatedAt))
            csv.AppendLine($"{line.PaymentIntentId},{line.GrossAmount},{line.CommissionAmount},{line.ProviderFeeAmount},{line.NetAmount}");
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"settlement-{batch.PeriodDate:yyyyMMdd}-{batch.Id:N}.csv");
    }
    [HttpGet("{organizationId:guid}"), RequirePermission("settlement.view")]
    public async Task<IActionResult> List(Guid organizationId, CancellationToken ct) => await tenantAccess.CanAccessOrganizationAsync(organizationId, ct) ? ApiResult.Success(await service.GetAllAsync(organizationId, ct)) : Forbid();
    [HttpPost("{organizationId:guid}/generate"), RequirePermission("settlement.generate")]
    public async Task<IActionResult> Generate(Guid organizationId, [FromQuery] DateOnly date, [FromQuery] string currency, CancellationToken ct) => await tenantAccess.CanAccessOrganizationAsync(organizationId, ct) ? ApiResult.Success(await service.GenerateAsync(organizationId, date, currency, Actor(), ct)) : Forbid();
    [HttpPost("{id:guid}/transition"), RequirePermission("settlement.approve")]
    public async Task<IActionResult> Transition(Guid id, [FromBody] SettlementTransition request, CancellationToken ct) { try { var existing = await service.GetAsync(id, ct); if (existing is null) return ApiResult.NotFound(); if (!await tenantAccess.CanAccessOrganizationAsync(existing.OrganizationId, ct)) return Forbid(); var batch = await service.TransitionAsync(id, request.Status, Actor(), request.PayoutReference, ct); await hub.Clients.All.SendAsync("ReceiveSettlementUpdate", new { batch.Id, batch.OrganizationId, batch.Status, batch.NetAmount, batch.Currency }, ct); return ApiResult.Success(batch); } catch (InvalidOperationException ex) { return ApiResult.BadRequest(ex.Message); } }
    private Guid Actor() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new InvalidOperationException("Authenticated actor is required.");
    private static string Escape(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : $"\"{value.Replace("\"", "\"\"")}\"";
}
public sealed record SettlementTransition(string Status, string? PayoutReference);
