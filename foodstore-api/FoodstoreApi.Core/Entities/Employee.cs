using FoodstoreApi.Core.Entities.Identity;

namespace FoodstoreApi.Core.Entities;

public partial class Employee : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Guid? BranchId { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime? HireDate { get; set; }
    public short Status { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ApplicationRole Role { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<Order> OrderEmployees { get; set; } = new List<Order>();
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<Order> OrderUpdatedByNavigations { get; set; } = new List<Order>();
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
