namespace FoodstoreApi.Usecase.DTOs.Finance;

public sealed record WalletBalanceDto(Guid OrganizationId, string Currency, decimal Available, decimal Pending, decimal Held, decimal Total);
public sealed record LedgerJournalDto(Guid Id, string EntryType, string SourceType, Guid SourceId, string Description, string Currency, DateTime PostedAt, decimal Debits, decimal Credits);
public sealed record LedgerAccountDto(Guid Id, Guid? OrganizationId, string Code, string Name, string AccountType, string Currency);
