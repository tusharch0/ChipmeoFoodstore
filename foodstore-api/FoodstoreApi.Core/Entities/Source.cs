namespace FoodstoreApi.Core.Entities;

public partial class Source : IAuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid? BranchId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    public virtual Branch? Branch { get; set; }
}
