using FoodstoreApi.Core.Entities;

namespace FoodstoreApi.Usecase.Interfaces;

public interface IOrganizationRepository
{
    Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveMembershipAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<OrganizationMembership?> GetActiveMembershipAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> BranchBelongsToOrganizationAsync(Guid branchId, Guid organizationId, CancellationToken cancellationToken = default);
    Task<Organization> CreateAsync(Organization organization, CancellationToken cancellationToken = default);
    Task<Branch> AddBranchAsync(Branch branch, CancellationToken cancellationToken = default);
    Task<Branch?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Branch> UpdateBranchAsync(Branch branch, CancellationToken cancellationToken = default);
}
