using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.Hubs;
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

        var results = await payments.PollPendingAsync(ct);

        foreach (var result in results.Where(r => r.Confirmed && r.OrderId.HasValue))
        {
            var payload = new { Id = result.OrderId, Status = result.OrderStatus, PaymentIntentId = result.IntentId };
            await hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", payload, ct);
            await hubContext.Clients.All.SendAsync("ReceivePaymentUpdate", payload, ct);
            // A confirmed online payment releases the order to the kitchen.
            await hubContext.Clients.Group("Kitchen").SendAsync("ReceiveNewOrder", payload, ct);
        }
    }
}
