namespace FoodstoreApi.Core.Entities;

/// <summary>
/// An immutable record of a provider callback/webhook or a status poll. Doubles as the durable
/// processing queue: rows with a null <see cref="ProcessingOutcome"/> are unprocessed and picked
/// up by the payment worker. Provider event ids are unique per provider for replay protection.
/// </summary>
public partial class PaymentEvent : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid? PaymentIntentId { get; set; }

    public string Provider { get; set; } = null!;

    /// <summary>Provider-side event id (or synthesized id for status polls). Unique per provider.</summary>
    public string ProviderEventId { get; set; } = null!;

    /// <summary>Provider event/status type, e.g. <c>succeeded</c>, <c>failed</c>.</summary>
    public string EventType { get; set; } = null!;

    public bool SignatureValid { get; set; }
    public string PayloadJson { get; set; } = "{}";

    public DateTime ReceivedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Null while unprocessed; set to the terminal handling result (e.g. <c>applied</c>, <c>duplicate-ignored</c>).</summary>
    public string? ProcessingOutcome { get; set; }
    public int Attempts { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    public virtual PaymentIntent? PaymentIntent { get; set; }
}
