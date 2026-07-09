using FoodstoreApi.Core.Constants;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodstoreApi.Web.Controllers;

/// <summary>
/// Untrusted provider callback ingress. Validates and durably stores the event, then returns fast.
/// State transitions are applied asynchronously by the payment worker.
/// </summary>
[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class PaymentWebhookController(IPaymentService service, ILogger<PaymentWebhookController> logger) : ControllerBase
{
    [HttpPost("intasend")]
    public async Task<IActionResult> IntaSend(CancellationToken ct)
    {
        string body;
        using (var reader = new StreamReader(Request.Body))
            body = await reader.ReadToEndAsync(ct);

        var headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        var result = await service.IngestWebhookAsync(PaymentProviders.IntaSend, body, headers, ct);
        logger.LogInformation("IntaSend webhook ingested: {Outcome}", result.Outcome);

        // Acknowledge accepted/deduplicated deliveries so the provider stops retrying; reject untrusted ones.
        return result.Outcome switch
        {
            "rejected" or "unknown-provider" => BadRequest(new { status = result.Outcome }),
            _ => Ok(new { status = result.Outcome })
        };
    }
}
