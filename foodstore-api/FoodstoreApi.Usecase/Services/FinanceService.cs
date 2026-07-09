using FoodstoreApi.Usecase.DTOs.Finance;
using FoodstoreApi.Usecase.Interfaces;
namespace FoodstoreApi.Usecase.Services;
public sealed class FinanceService(ILedgerRepository repository) : IFinanceService { public Task<IReadOnlyList<LedgerAccountDto>> GetAccountsAsync(Guid organizationId, CancellationToken ct = default) => repository.GetAccountsAsync(organizationId, ct); public Task<WalletBalanceDto> GetWalletAsync(Guid branchId, CancellationToken ct = default) => repository.GetWalletAsync(branchId, ct); public Task<IReadOnlyList<LedgerJournalDto>> GetJournalAsync(Guid branchId, DateTime? from, DateTime? to, CancellationToken ct = default) => repository.GetJournalAsync(branchId, from, to, ct); }
