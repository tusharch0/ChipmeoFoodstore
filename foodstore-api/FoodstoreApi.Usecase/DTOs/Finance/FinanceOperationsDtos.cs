namespace FoodstoreApi.Usecase.DTOs.Finance;
public sealed record CreateAdjustmentRequest(Guid OrganizationId, Guid LedgerAccountId, decimal Amount, string Currency, string ReasonCode, string Reason);
public sealed record ResolveReconciliationRequest(string Resolution, string? EvidenceUrl);
public sealed record CreateReconciliationRequest(Guid OrganizationId, Guid? SettlementBatchId, string SourceType, string SourceReference, decimal InternalAmount, decimal ExternalAmount, string Currency, string? EvidenceUrl);
public sealed record FinanceExceptionDto(Guid Id, string Type, Guid? BranchId, string Reference, string Status, decimal? Amount, string Currency, string? Detail, DateTime OccurredAt);
