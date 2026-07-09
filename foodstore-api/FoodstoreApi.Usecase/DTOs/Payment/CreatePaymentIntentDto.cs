namespace FoodstoreApi.Usecase.DTOs.Payment;

public sealed record CreatePaymentIntentDto(
    Guid OrderId,
    string Phone,
    string? CustomerName = null,
    string? CustomerEmail = null
);

public sealed record RefundPaymentDto(string Reason);

public sealed record PaymentSearchFilter(
    Guid? OrderId = null,
    string? OrderCode = null,
    string? Phone = null,
    string? ProviderReference = null,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20
);
