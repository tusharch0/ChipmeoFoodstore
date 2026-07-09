namespace FoodstoreApi.Core.Entities;

/// <summary>
/// A restaurant business operating on the MezaFlow platform.
/// </summary>
public class Organization : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string CurrencyCode { get; set; } = "KES";
    public string TimeZone { get; set; } = "Africa/Nairobi";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public virtual ICollection<OrganizationMembership> Memberships { get; set; } = new List<OrganizationMembership>();
}
