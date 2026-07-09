namespace FoodstoreApi.Core.Entities.Finance;

/// <summary>Chart-of-accounts node. Posted journal lines reference this immutable identifier.</summary>
public sealed class LedgerAccount : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid? OrganizationId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string AccountType { get; set; } = null!;
    public string Currency { get; set; } = "KES";
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
