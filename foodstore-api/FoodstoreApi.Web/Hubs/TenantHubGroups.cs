namespace FoodstoreApi.Web.Hubs;

public static class TenantHubGroups
{
    public static string Branch(Guid branchId) => $"branch:{branchId:N}";
    public static string Organization(Guid organizationId) => $"organization:{organizationId:N}";
}
