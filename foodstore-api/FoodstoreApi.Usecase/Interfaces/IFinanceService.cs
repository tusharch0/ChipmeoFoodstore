using FoodstoreApi.Usecase.DTOs.Finance;
namespace FoodstoreApi.Usecase.Interfaces;
public interface IFinanceService { Task<IReadOnlyList<LedgerAccountDto>> GetAccountsAsync(Guid organizationId, CancellationToken ct = default); Task<WalletBalanceDto> GetWalletAsync(Guid branchId, CancellationToken ct = default); Task<IReadOnlyList<LedgerJournalDto>> GetJournalAsync(Guid branchId, DateTime? from, DateTime? to, CancellationToken ct = default); }
