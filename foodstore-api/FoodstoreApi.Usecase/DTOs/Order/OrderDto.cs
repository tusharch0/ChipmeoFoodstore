namespace FoodstoreApi.Usecase.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? KitchenRouting { get; set; }
    public Guid? SourceId { get; set; }
    public string? SourceName { get; set; }
    public Guid? EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid? DiscountId { get; set; }
    public string? DiscountCode { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public decimal SubtotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? QrPaymentUrl { get; set; }
    public DateTime? PaidAt { get; set; }
    public string Status { get; set; } = "pending";
    public string? Note { get; set; }
    public DateTime? PrintedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public List<OrderItemDto> Items { get; set; } = new();
    public List<OrderStatusHistoryDto> History { get; set; } = new();
}
