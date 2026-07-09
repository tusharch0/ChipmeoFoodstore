namespace FoodstoreApi.Core.Entities;

public partial class Addon : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public bool? IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual ICollection<MenuItemAddon> MenuItemAddons { get; set; } = new List<MenuItemAddon>();
    public virtual ICollection<OrderItemAddon> OrderItemAddons { get; set; } = new List<OrderItemAddon>();
}
