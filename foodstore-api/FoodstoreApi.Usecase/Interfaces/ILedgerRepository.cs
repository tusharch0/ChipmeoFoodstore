using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Usecase.DTOs.Finance;

namespace FoodstoreApi.Usecase.Interfaces;

public sealed record FinancialPolicySnapshot(decimal Commission, decimal ProviderFee, string FeeBearer, string Currency);

public interface ILedgerRepository
{
    Task<bool> ExistsForSourceAsync(string sourceType, Guid sourceId, CancellationToken ct = default);
    Task<FinancialPolicySnapshot> GetPolicyAsync(Guid branchId, decimal amount, CancellationToken ct = default);
    Task StagePaymentJournalAsync(PaymentIntent intent, Order order, FinancialPolicySnapshot policy, CancellationToken ct = default);
    Task StageRefundJournalAsync(PaymentIntent intent, Order order, CancellationToken ct = default);
    Task StageSettlementPayoutJournalAsync(SettlementBatch batch, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerAccountDto>> GetAccountsAsync(Guid organizationId, CancellationToken ct = default);
    Task<WalletBalanceDto> GetWalletAsync(Guid branchId, CancellationToken ct = default);
    Task<IReadOnlyList<LedgerJournalDto>> GetJournalAsync(Guid branchId, DateTime? from, DateTime? to, CancellationToken ct = default);
}
