using FoodstoreApi.Core.Entities.Finance;
using FoodstoreApi.Usecase.DTOs.Finance;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IFinanceOperationsService
{
    Task<IReadOnlyList<FinanceExceptionDto>> ExceptionsAsync(Guid organizationId, CancellationToken ct = default);
    Task<IReadOnlyList<FinanceExceptionDto>> UnmatchedPaymentsAsync(CancellationToken ct = default);
    Task<FinanceAdjustmentRequest?> GetAdjustmentAsync(Guid id, CancellationToken ct = default);
    Task<FinanceAdjustmentRequest> RequestAdjustmentAsync(CreateAdjustmentRequest request, Guid actorId, CancellationToken ct = default);
    Task<FinanceAdjustmentRequest> ApproveAdjustmentAsync(Guid id, Guid actorId, CancellationToken ct = default);
    Task<IReadOnlyList<FinanceAdjustmentRequest>> AdjustmentsAsync(Guid organizationId, CancellationToken ct = default);
    Task<ReconciliationCase?> GetCaseAsync(Guid id, CancellationToken ct = default);
    Task<ReconciliationCase> CreateCaseAsync(CreateReconciliationRequest request, Guid actorId, CancellationToken ct = default);
    Task<ReconciliationCase> ResolveCaseAsync(Guid id, ResolveReconciliationRequest request, Guid actorId, CancellationToken ct = default);
    Task<IReadOnlyList<ReconciliationCase>> CasesAsync(Guid organizationId, CancellationToken ct = default);
}
