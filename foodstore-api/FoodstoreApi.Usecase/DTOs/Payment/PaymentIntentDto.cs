namespace FoodstoreApi.Usecase.DTOs.Payment;

public class PaymentIntentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string? OrderCode { get; set; }
    public Guid? BranchId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string Provider { get; set; } = null!;
    public string? ProviderReference { get; set; }
    public string? CheckoutId { get; set; }
    public string Status { get; set; } = null!;
    public string? CustomerPhone { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class PaymentEventDto
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = null!;
    public string ProviderEventId { get; set; } = null!;
    public string EventType { get; set; } = null!;
    public bool SignatureValid { get; set; }
    public string? ProcessingOutcome { get; set; }
    public int Attempts { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public sealed class PaymentIntentDetailDto : PaymentIntentDto
{
    public List<PaymentEventDto> Events { get; set; } = new();
}
