namespace FoodstoreApi.Core.Constants;

/// <summary>
/// Lifecycle states for a <c>PaymentIntent</c> and the allowed transitions between them.
/// Terminal money-moving states (<see cref="Succeeded"/>, <see cref="Failed"/>, <see cref="Expired"/>,
/// <see cref="Cancelled"/>) reject further confirmation, which is the core idempotency guard.
/// </summary>
public static class PaymentIntentStatus
{
    public const string Created = "created";
    public const string Pending = "pending";
    public const string Succeeded = "succeeded";
    public const string Failed = "failed";
    public const string Expired = "expired";
    public const string Cancelled = "cancelled";
    public const string RefundPending = "refund_pending";
    public const string Refunded = "refunded";

    /// <summary>States that are still awaiting a provider outcome.</summary>
    public static readonly HashSet<string> OpenSet = [Created, Pending];

    /// <summary>States from which no further payment confirmation may occur.</summary>
    public static readonly HashSet<string> ConfirmedSet = [Succeeded, RefundPending, Refunded];

    public static readonly Dictionary<string, HashSet<string>> Transitions = new()
    {
        [Created] = [Pending, Cancelled, Failed, Expired],
        [Pending] = [Succeeded, Failed, Expired, Cancelled],
        [Succeeded] = [RefundPending],
        [RefundPending] = [Refunded, Succeeded],
    };
}
