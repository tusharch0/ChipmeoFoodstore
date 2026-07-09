using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Hubs;
using FoodstoreApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace FoodstoreApi.Web.BackgroundServices;

/// <summary>
/// Durable payment worker. On an interval it drains stored-but-unprocessed webhook events and
/// reconciles stale pending intents against the provider, then broadcasts confirmations so POS,
/// customer, and kitchen surfaces update in real time. Runs outside any HTTP request, so the tenant
/// context resolves as system and query filters are bypassed.
/// </summary>
public class PaymentEventWorker(
    IServiceScopeFactory scopeFactory,
    IHubContext<AppHub> hubContext,
    ILogger<PaymentEventWorker> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PaymentEventWorker started");
        using var timer = new PeriodicTimer(Interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "PaymentEventWorker cycle failed");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        logger.LogInformation("PaymentEventWorker stopping");
    }

    private async Task ProcessOnceAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var payments = scope.ServiceProvider.GetRequiredService<IPaymentService>();
        var db = scope.ServiceProvider.GetRequiredService<StoreDbContext>();

        var results = await payments.PollPendingAsync(ct);

        foreach (var result in results.Where(r => r.Confirmed && r.OrderId.HasValue))
        {
            var branchId = await db.Orders.AsNoTracking()
                .Where(order => order.Id == result.OrderId!.Value)
                .Select(order => order.BranchId)
                .SingleOrDefaultAsync(ct);
            if (!branchId.HasValue)
                continue;

            var payload = new { Id = result.OrderId, Status = result.OrderStatus, PaymentIntentId = result.IntentId };
            var clients = hubContext.Clients.Group(TenantHubGroups.Branch(branchId.Value));
            await clients.SendAsync("ReceiveOrderUpdate", payload, ct);
            await clients.SendAsync("ReceivePaymentUpdate", payload, ct);
            // A confirmed online payment releases the order to the kitchen.
            await clients.SendAsync("ReceiveNewOrder", payload, ct);
        }
    }
}
