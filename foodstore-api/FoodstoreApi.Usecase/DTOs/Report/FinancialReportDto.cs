namespace FoodstoreApi.Usecase.DTOs.Report;

public sealed record FinancialReportRequest(Guid OrganizationId, DateOnly FromDate, DateOnly ToDate, Guid? BranchId, Guid ActorId, bool IsExport);

public sealed record FinancialReportDto(
    Guid OrganizationId,
    string Currency,
    string TimeZone,
    DateOnly FromDate,
    DateOnly ToDate,
    FinancialReportTotals Totals,
    IReadOnlyList<BranchFinancialSummary> Branches,
    FinancialExceptionSummary Exceptions);

public sealed record FinancialReportTotals(
    int Orders,
    int SuccessfulPayments,
    decimal PaymentConversionRate,
    decimal GrossSales,
    decimal Refunds,
    decimal Commissions,
    decimal ProviderFees,
    decimal NetSales,
    decimal WalletPosition,
    decimal PendingSettlement,
    int SettlementCount);

public sealed record BranchFinancialSummary(
    Guid BranchId,
    string BranchName,
    int Orders,
    int SuccessfulPayments,
    decimal PaymentConversionRate,
    decimal GrossSales,
    decimal Refunds,
    decimal NetSales,
    decimal WalletPosition);

public sealed record FinancialExceptionSummary(
    int FailedPayments,
    int RefundsAwaitingAction,
    int NegativeOrHeldWallets,
    int SettlementFailures,
    int ReconciliationDiscrepancies);
