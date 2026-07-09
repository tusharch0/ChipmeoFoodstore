using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Web.BackgroundServices;

/// <summary>Generates one idempotent daily draft per organization whose finance policy enables automatic settlements.</summary>
public sealed class SettlementWorker(IServiceScopeFactory scopeFactory, ILogger<SettlementWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do { try { await GenerateAsync(stoppingToken); } catch (Exception ex) { logger.LogError(ex, "Settlement generation cycle failed"); } }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
    private async Task GenerateAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StoreDbContext>();
        var service = scope.ServiceProvider.GetRequiredService<ISettlementService>();
        var policies = await db.FinancePolicies.AsNoTracking().Where(p => p.IsActive && p.SettlementMode == "automatic" && p.SettlementCalendar == "daily").Select(p => p.OrganizationId).ToListAsync(ct);
        foreach (var organizationId in policies)
        {
            var organization = await db.Organizations.AsNoTracking().Where(o => o.Id == organizationId)
                .Select(o => new { o.CurrencyCode, o.TimeZone }).SingleAsync(ct);
            var zone = ResolveTimeZone(organization.TimeZone);
            var date = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone).Date.AddDays(-1));
            var currency = organization.CurrencyCode;
            await service.GenerateAsync(organizationId, date, currency, Guid.Empty, ct);
        }
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZone)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(timeZone); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time"); }
    }
}
