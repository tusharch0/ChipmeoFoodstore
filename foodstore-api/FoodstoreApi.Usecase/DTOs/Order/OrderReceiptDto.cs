namespace FoodstoreApi.Usecase.DTOs.Order;

public sealed record OrderReceiptDto(
    Guid OrderId,
    string OrderCode,
    Guid? BranchId,
    string? BranchName,
    string? BranchAddress,
    string? CustomerName,
    string? CustomerPhone,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal VatAmount,
    decimal TotalAmount,
    string Currency,
    string Status,
    DateTime? PaidAt,
    Guid? PaymentIntentId,
    string? Provider,
    string? ProviderReference,
    IReadOnlyList<OrderReceiptLineDto> Items);

public sealed record OrderReceiptLineDto(string Name, int Quantity, decimal UnitPrice, decimal TotalPrice);
