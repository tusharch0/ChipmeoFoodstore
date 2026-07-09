using FoodstoreApi.Core.Entities.Finance;
namespace FoodstoreApi.Usecase.Interfaces;
public interface ISettlementRepository { Task<SettlementBatch?> GetAsync(Guid id, CancellationToken ct = default); Task<IReadOnlyList<SettlementBatch>> GetAllAsync(Guid organizationId, CancellationToken ct = default); Task<SettlementBatch> GenerateAsync(Guid organizationId, DateOnly date, string currency, Guid actorId, CancellationToken ct = default); Task SaveAsync(SettlementBatch batch, CancellationToken ct = default); }
