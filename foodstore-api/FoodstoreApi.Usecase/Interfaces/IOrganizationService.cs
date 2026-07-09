using FoodstoreApi.Usecase.DTOs.Organization;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IOrganizationService
{
    Task<IReadOnlyList<OrganizationDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default);
    Task<BranchDto?> AddBranchAsync(Guid organizationId, CreateBranchRequest request, CancellationToken cancellationToken = default);
}
