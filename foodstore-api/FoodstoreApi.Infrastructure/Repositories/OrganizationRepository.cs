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

    public Task<OrganizationMembership?> GetActiveMembershipAsync(Guid userId, Guid organizationId, CancellationToken cancellationToken = default) =>
        context.OrganizationMemberships.AsNoTracking().SingleOrDefaultAsync(
            e => e.UserId == userId && e.OrganizationId == organizationId && e.IsActive,
            cancellationToken);

    public async Task<OrganizationMembership> EnsureMembershipAsync(Guid userId, Guid organizationId, string role, CancellationToken cancellationToken = default)
    {
        var membership = await context.OrganizationMemberships.SingleOrDefaultAsync(
            e => e.UserId == userId && e.OrganizationId == organizationId, cancellationToken);
        if (membership is null)
        {
            membership = new OrganizationMembership
            {
                Id = Guid.NewGuid(), UserId = userId, OrganizationId = organizationId,
                Role = role, IsActive = true
            };
            context.OrganizationMemberships.Add(membership);
        }
        else
        {
            membership.Role = role;
            membership.IsActive = true;
        }
        await context.SaveChangesAsync(cancellationToken);
        return membership;
    }

    public async Task<IReadOnlyList<OrganizationMembership>> GetMembershipsAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        await context.OrganizationMemberships.AsNoTracking().Include(item => item.User).ThenInclude(user => user.Employee).ThenInclude(employee => employee!.Branch)
            .Where(item => item.OrganizationId == organizationId).OrderBy(item => item.User.Name).ToListAsync(cancellationToken);

    public async Task<OrganizationMembership?> UpdateMembershipAsync(Guid organizationId, Guid membershipId, string role, bool isActive, CancellationToken cancellationToken = default)
    {
        var membership = await context.OrganizationMemberships.Include(item => item.User).ThenInclude(user => user.Employee).ThenInclude(employee => employee!.Branch)
            .SingleOrDefaultAsync(item => item.Id == membershipId && item.OrganizationId == organizationId, cancellationToken);
        if (membership is null) return null;
        membership.Role = role;
        membership.IsActive = isActive;
        await context.SaveChangesAsync(cancellationToken);
        return membership;
    }

    public Task<bool> BranchBelongsToOrganizationAsync(Guid branchId, Guid organizationId, CancellationToken cancellationToken = default) =>
        context.Branches.IgnoreQueryFilters().AnyAsync(e => e.Id == branchId && e.OrganizationId == organizationId, cancellationToken);

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

    public Task<Branch?> GetBranchByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Branches.IgnoreQueryFilters().SingleOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<Branch> UpdateBranchAsync(Branch branch, CancellationToken cancellationToken = default)
    {
        context.Branches.Update(branch);
        await context.SaveChangesAsync(cancellationToken);
        return branch;
    }
}
