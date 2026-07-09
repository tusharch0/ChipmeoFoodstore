using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Usecase.DTOs.Finance;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.Services;
using Xunit;

namespace FoodstoreApi.Tests;

public sealed class FinanceWorkflowTests
{
    [Fact]
    public async Task PaymentPostingIsIdempotent()
    {
        var repository = new FakeLedgerRepository { Exists = true };
        var service = new LedgerService(repository);
        await service.StagePaymentConfirmationAsync(new PaymentIntent { Id = Guid.NewGuid(), BranchId = Guid.NewGuid(), Amount = 100m, Currency = "KES" }, new Order());
        Assert.Equal(0, repository.PaymentPosts);
    }

    [Fact]
    public async Task PaymentPostingRejectsCurrencyMismatch()
    {
        var service = new LedgerService(new FakeLedgerRepository { Policy = new FinancialPolicySnapshot(0m, 0m, "restaurant", "USD") });
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StagePaymentConfirmationAsync(
            new PaymentIntent { Id = Guid.NewGuid(), BranchId = Guid.NewGuid(), Amount = 100m, Currency = "KES" }, new Order()));
    }

    [Fact]
    public async Task SettlementEnforcesMakerChecker()
    {
        var actor = Guid.NewGuid();
        var repository = new FakeSettlementRepository(new SettlementBatch { Id = Guid.NewGuid(), Status = "draft", CreatedBy = actor });
        var service = new SettlementService(repository, new LedgerService(new FakeLedgerRepository()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransitionAsync(repository.Batch.Id, "reviewed", actor, null));
    }

    [Fact]
    public async Task PaidSettlementRequiresStablePayoutReference()
    {
        var repository = new FakeSettlementRepository(new SettlementBatch { Id = Guid.NewGuid(), Status = "submitted", NetAmount = 50m });
        var service = new SettlementService(repository, new LedgerService(new FakeLedgerRepository()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.TransitionAsync(repository.Batch.Id, "paid", Guid.NewGuid(), null));
    }

    private sealed class FakeLedgerRepository : ILedgerRepository
    {
        public bool Exists { get; init; }
        public int PaymentPosts { get; private set; }
        public FinancialPolicySnapshot Policy { get; init; } = new(0m, 0m, "restaurant", "KES");
        public Task<bool> ExistsForSourceAsync(string sourceType, Guid sourceId, CancellationToken ct = default) => Task.FromResult(Exists);
        public Task<FinancialPolicySnapshot> GetPolicyAsync(Guid branchId, decimal amount, CancellationToken ct = default) => Task.FromResult(Policy);
        public Task StagePaymentJournalAsync(PaymentIntent intent, Order order, FinancialPolicySnapshot policy, CancellationToken ct = default) { PaymentPosts++; return Task.CompletedTask; }
        public Task StageRefundJournalAsync(PaymentIntent intent, Order order, CancellationToken ct = default) => Task.CompletedTask;
        public Task StageSettlementPayoutJournalAsync(SettlementBatch batch, CancellationToken ct = default) => Task.CompletedTask;
        public Task<IReadOnlyList<LedgerAccountDto>> GetAccountsAsync(Guid organizationId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<LedgerAccountDto>>([]);
        public Task<WalletBalanceDto> GetWalletAsync(Guid branchId, CancellationToken ct = default) => Task.FromResult(new WalletBalanceDto(Guid.Empty, "KES", 0m, 0m, 0m, 0m));
        public Task<IReadOnlyList<LedgerJournalDto>> GetJournalAsync(Guid branchId, DateTime? from, DateTime? to, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<LedgerJournalDto>>([]);
    }

    private sealed class FakeSettlementRepository(SettlementBatch batch) : ISettlementRepository
    {
        public SettlementBatch Batch { get; } = batch;
        public Task<SettlementBatch?> GetAsync(Guid id, CancellationToken ct = default) => Task.FromResult<SettlementBatch?>(id == Batch.Id ? Batch : null);
        public Task<IReadOnlyList<SettlementBatch>> GetAllAsync(Guid organizationId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<SettlementBatch>>([Batch]);
        public Task<SettlementBatch> GenerateAsync(Guid organizationId, DateOnly date, string currency, Guid actorId, CancellationToken ct = default) => Task.FromResult(Batch);
        public Task SaveAsync(SettlementBatch batch, CancellationToken ct = default) => Task.CompletedTask;
    }
}
