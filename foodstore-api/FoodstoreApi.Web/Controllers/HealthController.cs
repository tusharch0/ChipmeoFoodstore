using Microsoft.AspNetCore.Mvc;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("/api/health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() { return ApiResult.Success(new { status = "healthy", timestamp = DateTime.UtcNow }); }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready([FromServices] StoreDbContext context, CancellationToken cancellationToken)
    {
        if (!await context.Database.CanConnectAsync(cancellationToken))
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Failure(
                new ErrorDetail { Code = "DEPENDENCY_UNAVAILABLE", Message = "Database is unavailable." }));

        return ApiResult.Success(new { status = "ready", timestamp = DateTime.UtcNow });
    }
}




