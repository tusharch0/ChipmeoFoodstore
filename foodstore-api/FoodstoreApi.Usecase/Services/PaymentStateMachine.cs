using FoodstoreApi.Core.Constants;

namespace FoodstoreApi.Usecase.Services;

/// <summary>
/// Enforces the <see cref="PaymentIntentStatus"/> transition rules so payment state can only move
/// along the agreed lifecycle. Callers use <see cref="CanTransition"/> to make confirmation idempotent.
/// </summary>
public static class PaymentStateMachine
{
    public static bool CanTransition(string from, string to)
    {
        if (string.Equals(from, to, StringComparison.Ordinal))
            return false;
        return PaymentIntentStatus.Transitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }

    public static bool IsOpen(string status) => PaymentIntentStatus.OpenSet.Contains(status);

    public static bool IsConfirmed(string status) => PaymentIntentStatus.ConfirmedSet.Contains(status);
}
