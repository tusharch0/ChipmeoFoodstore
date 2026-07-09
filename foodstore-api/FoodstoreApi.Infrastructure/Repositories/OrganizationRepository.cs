using FoodstoreApi.Core.Entities;
using FoodstoreApi.Infrastructure.Data;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FoodstoreApi.Infrastructure.Repositories;

public class OrganizationRepository(StoreDbContext context) : IOrganizationRepository
{
    public async Task<IReadOnlyList<Organization>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Organizations.AsNoTracking().Include(e => e.Branches).OrderBy(e => e.Name).ToListAsync(cancellationToken);

    public Task<Organization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Organizations.AsNoTracking().Include(e => e.Branches).SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken = default) =>
        context.Organizations.AnyAsync(e => e.Slug == slug, cancellationToken);

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Users.AnyAsync(e => e.Id == userId, cancellationToken);

    public Task<bool> HasActiveMembershipAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default) =>
        context.OrganizationMemberships.AnyAsync(
            e => e.UserId == userId && e.OrganizationId == organizationId && e.IsActive,
            cancellationToken);

    public async Task<Organization> CreateAsync(Organization organization, CancellationToken cancellationToken = default)
    {
        context.Organizations.Add(organization);
        await context.SaveChangesAsync(cancellationToken);
        return organization;
    }

    public async Task<Branch> AddBranchAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        context.Branches.Add(branch);
        await context.SaveChangesAsync(cancellationToken);
        return branch;
    }
}
