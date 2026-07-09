namespace FoodstoreApi.Core.Entities;

public partial class Order : IAuditableEntity
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = null!;
    public Guid? SourceId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? EmployeeId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? DiscountId { get; set; }
    public decimal? SubtotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? VatAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? QrPaymentUrl { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public DateTime? PrintedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Discount? Discount { get; set; }
    public virtual Customer? Customer { get; set; }
    public virtual Employee? Employee { get; set; }
    public virtual Source? Source { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual Employee? UpdatedByNavigation { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
