namespace FoodstoreApi.Usecase.Interfaces;

/// <summary>Applies organization and branch scope checks before cross-branch reads or mutations.</summary>
public interface ITenantAccessService
{
    Task<bool> CanAccessOrganizationAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
}
