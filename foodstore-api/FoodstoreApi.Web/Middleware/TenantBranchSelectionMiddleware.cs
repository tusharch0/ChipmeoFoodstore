using System.Security.Claims;
using FoodstoreApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Web.Middleware;

public sealed class TenantBranchSelectionMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Branch-ID";
    public const string BranchItem = "TenantBranchId";
    public const string OrganizationItem = "TenantOrganizationId";

    public async Task InvokeAsync(HttpContext context, StoreDbContext db)
    {
        var raw = context.Request.Headers[HeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(raw)) { await next(context); return; }
        if (!Guid.TryParse(raw, out var requestedBranch)) { context.Response.StatusCode = StatusCodes.Status400BadRequest; return; }

        var claimBranch = Guid.TryParse(context.User.FindFirstValue("branch_id"), out var branchId) ? branchId : (Guid?)null;
        var claimOrganization = Guid.TryParse(context.User.FindFirstValue("organization_id"), out var organizationId) ? organizationId : (Guid?)null;
        var isOwner = string.Equals(context.User.FindFirstValue("organization_role"), "owner", StringComparison.OrdinalIgnoreCase);
        var isPlatform = bool.TryParse(context.User.FindFirstValue("is_platform_operator"), out var platform) && platform;
        if (claimBranch == requestedBranch)
        {
            context.Items[BranchItem] = requestedBranch;
            if (claimOrganization.HasValue) context.Items[OrganizationItem] = claimOrganization.Value;
            await next(context); return;
        }

        var branch = await db.Branches.IgnoreQueryFilters().AsNoTracking().Where(item => item.Id == requestedBranch && item.IsActive)
            .Select(item => new { item.Id, item.OrganizationId }).SingleOrDefaultAsync(context.RequestAborted);
        if (branch is null || (!isPlatform && (!isOwner || claimOrganization != branch.OrganizationId)))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden; return;
        }
        context.Items[BranchItem] = branch.Id;
        context.Items[OrganizationItem] = branch.OrganizationId;
        await next(context);
    }
}
