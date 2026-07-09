using FoodstoreApi.Usecase.Interfaces;

namespace FoodstoreApi.Usecase.Services;

public sealed class TenantAccessService(ITenantContext tenantContext, IOrganizationRepository organizationRepository) : ITenantAccessService
{
    public Task<bool> CanAccessOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        var allowed = tenantContext.IsSystem || tenantContext.IsPlatformOperator ||
            (tenantContext.IsOrganizationOwner && tenantContext.OrganizationId == organizationId);
        return Task.FromResult(allowed);
    }

    public async Task<bool> CanAccessBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        if (tenantContext.IsSystem || tenantContext.IsPlatformOperator || tenantContext.BranchId == branchId)
            return true;

        return tenantContext.IsOrganizationOwner && tenantContext.OrganizationId.HasValue &&
            await organizationRepository.BranchBelongsToOrganizationAsync(branchId, tenantContext.OrganizationId.Value, cancellationToken);
    }
}
