using FoodstoreApi.Core.Entities;
using FoodstoreApi.Usecase.DTOs.Organization;
using FoodstoreApi.Usecase.Interfaces;

namespace FoodstoreApi.Usecase.Services;

public class OrganizationService(IOrganizationRepository repository) : IOrganizationService
{
    public async Task<IReadOnlyList<OrganizationDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken)).Select(Map).ToList();

    public async Task<OrganizationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organization = await repository.GetByIdAsync(id, cancellationToken);
        return organization is null ? null : Map(organization);
    }

    public async Task<OrganizationDto> CreateAsync(CreateOrganizationRequest request, CancellationToken cancellationToken = default)
    {
        ValidateNameAndCode(request.Name, request.Slug, request.PrimaryBranchName, request.PrimaryBranchCode);
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await repository.SlugExistsAsync(slug, cancellationToken))
            throw new InvalidOperationException("Organization slug already exists.");
        if (request.OwnerUserId.HasValue && !await repository.UserExistsAsync(request.OwnerUserId.Value, cancellationToken))
            throw new InvalidOperationException("Owner user does not exist.");

        var organization = new Organization
        {
            Id = Guid.NewGuid(), Name = request.Name.Trim(), Slug = slug,
            LegalName = request.LegalName?.Trim(), TaxId = request.TaxId?.Trim()
        };
        organization.Branches.Add(new Branch
        {
            Id = Guid.NewGuid(), Name = request.PrimaryBranchName.Trim(), Code = request.PrimaryBranchCode.Trim().ToUpperInvariant(),
            Address = request.Address?.Trim(), City = request.City?.Trim(), Phone = request.Phone?.Trim()
        });
        if (request.OwnerUserId.HasValue)
        {
            organization.Memberships.Add(new OrganizationMembership
            {
                Id = Guid.NewGuid(), UserId = request.OwnerUserId.Value, Role = "owner"
            });
        }

        return Map(await repository.CreateAsync(organization, cancellationToken));
    }

    public async Task<BranchDto?> AddBranchAsync(Guid organizationId, CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        ValidateNameAndCode(request.Name, request.Code, request.Name, request.Code);
        var organization = await repository.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null) return null;
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (organization.Branches.Any(e => e.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Branch code already exists for this organization.");

        var branch = await repository.AddBranchAsync(new Branch
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, Name = request.Name.Trim(), Code = normalizedCode,
            Address = request.Address?.Trim(), City = request.City?.Trim(), Phone = request.Phone?.Trim()
        }, cancellationToken);
        return Map(branch);
    }

    private static void ValidateNameAndCode(string name, string slug, string branchName, string branchCode)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(branchName))
            throw new InvalidOperationException("Organization and branch names are required.");
        if (string.IsNullOrWhiteSpace(slug) || slug.Any(c => !char.IsLetterOrDigit(c) && c != '-'))
            throw new InvalidOperationException("Slug must contain only letters, numbers, and hyphens.");
        if (string.IsNullOrWhiteSpace(branchCode))
            throw new InvalidOperationException("Branch code is required.");
    }

    private static OrganizationDto Map(Organization organization) => new(
        organization.Id, organization.Name, organization.Slug, organization.LegalName, organization.TaxId,
        organization.CurrencyCode, organization.TimeZone, organization.IsActive,
        organization.Branches.OrderBy(e => e.Name).Select(Map).ToList());

    private static BranchDto Map(Branch branch) => new(branch.Id, branch.Name, branch.Code, branch.Address, branch.City, branch.Phone, branch.IsActive);
}
