namespace FoodstoreApi.Core.Entities;

public partial class Combo : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
    public string Name { get; set; } = null!;
    public decimal ComboPrice { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
