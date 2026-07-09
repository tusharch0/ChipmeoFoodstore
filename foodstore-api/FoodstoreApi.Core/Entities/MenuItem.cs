namespace FoodstoreApi.Core.Entities;

public partial class MenuItem : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
    public Guid? CategoryId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool? IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual Category? Category { get; set; }
    public virtual ICollection<ComboItem> ComboItems { get; set; } = new List<ComboItem>();
    public virtual ICollection<MenuItemAddon> MenuItemAddons { get; set; } = new List<MenuItemAddon>();
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
