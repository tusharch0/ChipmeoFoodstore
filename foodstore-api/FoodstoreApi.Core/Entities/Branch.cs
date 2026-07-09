namespace FoodstoreApi.Core.Entities;

/// <summary>
/// A physical restaurant location belonging to an organization.
/// </summary>
public class Branch : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? OpeningHoursJson { get; set; }
    public string? TaxSettingsJson { get; set; }
    public string? KitchenRouting { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual ICollection<Source> Sources { get; set; } = new List<Source>();
}
