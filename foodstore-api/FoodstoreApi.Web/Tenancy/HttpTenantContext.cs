using System.Security.Claims;
using FoodstoreApi.Usecase.Interfaces;

namespace FoodstoreApi.Web.Tenancy;

public sealed class HttpTenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid? BranchId => Guid.TryParse(_user?.FindFirst("branch_id")?.Value, out var branchId)
        ? branchId
        : null;

    public bool IsPlatformOperator => bool.TryParse(_user?.FindFirst("is_platform_operator")?.Value, out var isPlatformOperator)
        && isPlatformOperator;

    // Work performed outside an HTTP request (migrations, seeders, and trusted workers) is platform-owned.
    public bool IsSystem => _user is null;

    public string CacheScope => IsSystem
        ? "system"
        : IsPlatformOperator
            ? "platform"
            : BranchId.HasValue
                ? $"branch:{BranchId.Value:N}"
                : "unscoped";
}
