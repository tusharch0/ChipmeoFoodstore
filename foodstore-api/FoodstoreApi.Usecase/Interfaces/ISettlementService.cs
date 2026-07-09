using FoodstoreApi.Core.Entities.Finance;
namespace FoodstoreApi.Usecase.Interfaces;
public interface ISettlementService { Task<SettlementBatch?> GetAsync(Guid id, CancellationToken ct = default); Task<SettlementBatch> GenerateAsync(Guid organizationId, DateOnly date, string currency, Guid actorId, CancellationToken ct = default); Task<SettlementBatch> TransitionAsync(Guid id, string target, Guid actorId, string? payoutReference, CancellationToken ct = default); Task<IReadOnlyList<SettlementBatch>> GetAllAsync(Guid organizationId, CancellationToken ct = default); }
