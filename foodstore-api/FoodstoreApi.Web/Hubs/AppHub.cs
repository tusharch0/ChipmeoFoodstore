using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using FoodstoreApi.Usecase.Interfaces;

namespace FoodstoreApi.Web.Hubs;

[Authorize]
public sealed class AppHub(IOrganizationRepository organizations) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var claimBranch = Guid.TryParse(Context.User?.FindFirstValue("branch_id"), out var branchId) ? branchId : (Guid?)null;
        var selectedBranch = Guid.TryParse(Context.GetHttpContext()?.Request.Query["branchId"], out var requestedBranch) ? requestedBranch : claimBranch;
        if (selectedBranch.HasValue && selectedBranch != claimBranch)
        {
            var isPlatform = bool.TryParse(Context.User?.FindFirstValue("is_platform_operator"), out var platform) && platform;
            var isOwner = string.Equals(Context.User?.FindFirstValue("organization_role"), "owner", StringComparison.OrdinalIgnoreCase);
            var selectedOrganizationId = Guid.TryParse(Context.User?.FindFirstValue("organization_id"), out var organization) ? organization : (Guid?)null;
            if (!isPlatform && (!isOwner || !selectedOrganizationId.HasValue || !await organizations.BranchBelongsToOrganizationAsync(selectedBranch.Value, selectedOrganizationId.Value)))
            {
                Context.Abort();
                return;
            }
        }
        if (selectedBranch.HasValue)
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantHubGroups.Branch(selectedBranch.Value));

        if (Guid.TryParse(Context.User?.FindFirstValue("organization_id"), out var organizationId))
            await Groups.AddToGroupAsync(Context.ConnectionId, TenantHubGroups.Organization(organizationId));

        await base.OnConnectedAsync();
    }
}
