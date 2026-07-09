using FoodstoreApi.Core.Entities.Identity;

namespace FoodstoreApi.Core.Entities;

/// <summary>
/// Grants a user access to a restaurant organization.
/// </summary>
public class OrganizationMembership : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = "owner";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual Organization Organization { get; set; } = null!;
    public virtual ApplicationUser User { get; set; } = null!;
}
