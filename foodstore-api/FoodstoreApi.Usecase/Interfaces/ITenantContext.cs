namespace FoodstoreApi.Usecase.Interfaces;

/// <summary>
/// Resolves the tenant scope for the current request without exposing HTTP concerns to application services.
/// </summary>
public interface ITenantContext
{
    Guid? BranchId { get; }
    bool IsPlatformOperator { get; }
    bool IsSystem { get; }
    string CacheScope { get; }
}
