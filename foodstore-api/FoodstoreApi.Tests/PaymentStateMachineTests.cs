using FoodstoreApi.Core.Constants;
using FoodstoreApi.Usecase.Services;
using Xunit;

namespace FoodstoreApi.Tests;

public sealed class PaymentStateMachineTests
{
    [Theory]
    [InlineData(PaymentIntentStatus.Created, PaymentIntentStatus.Pending)]
    [InlineData(PaymentIntentStatus.Pending, PaymentIntentStatus.Succeeded)]
    [InlineData(PaymentIntentStatus.Succeeded, PaymentIntentStatus.RefundPending)]
    [InlineData(PaymentIntentStatus.RefundPending, PaymentIntentStatus.Refunded)]
    public void AllowsConfiguredTransition(string from, string to) => Assert.True(PaymentStateMachine.CanTransition(from, to));

    [Theory]
    [InlineData(PaymentIntentStatus.Succeeded, PaymentIntentStatus.Succeeded)]
    [InlineData(PaymentIntentStatus.Succeeded, PaymentIntentStatus.Failed)]
    [InlineData(PaymentIntentStatus.Failed, PaymentIntentStatus.Succeeded)]
    public void RejectsDuplicateAndTerminalTransitions(string from, string to) => Assert.False(PaymentStateMachine.CanTransition(from, to));
}
