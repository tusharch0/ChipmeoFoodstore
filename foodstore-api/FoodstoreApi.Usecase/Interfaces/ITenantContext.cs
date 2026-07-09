namespace FoodstoreApi.Usecase.Interfaces;

/// <summary>
/// Resolves the tenant scope for the current request without exposing HTTP concerns to application services.
/// </summary>
public interface ITenantContext
{
    Guid? BranchId { get; }
    Guid? OrganizationId { get; }
    bool IsOrganizationOwner { get; }
    bool IsPlatformOperator { get; }
    bool IsSystem { get; }
    string CacheScope { get; }
}
