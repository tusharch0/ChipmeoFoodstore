namespace FoodstoreApi.Core.Entities;

/// <summary>
/// A single customer payment attempt against an order. This is the lifecycle source of truth
/// for money movement through a provider (e.g. IntaSend M-Pesa). A confirmed intent flips the
/// order to paid and writes a settled <see cref="Payment"/> record.
/// </summary>
public partial class PaymentIntent : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? BranchId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";

    /// <summary>Provider key, e.g. <c>intasend</c>.</summary>
    public string Provider { get; set; } = null!;

    /// <summary>Provider-side reference (IntaSend invoice/tracking id). Null until the charge is created.</summary>
    public string? ProviderReference { get; set; }

    /// <summary>Provider checkout/api-ref used to correlate STK push and webhooks.</summary>
    public string? CheckoutId { get; set; }

    /// <summary>Client-supplied idempotency key. Unique — a repeated submission returns the same intent.</summary>
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>Lifecycle state — see <c>PaymentIntentStatus</c>.</summary>
    public string Status { get; set; } = null!;

    public string? CustomerPhone { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual ICollection<PaymentEvent> Events { get; set; } = new List<PaymentEvent>();
}
