using Microsoft.AspNetCore.Identity;

namespace FoodstoreApi.Core.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public bool Banned { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual Employee? Employee { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual ICollection<OrganizationMembership> OrganizationMemberships { get; set; } = new List<OrganizationMembership>();
}
