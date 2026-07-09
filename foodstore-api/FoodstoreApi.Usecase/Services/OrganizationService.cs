using FoodstoreApi.Core.Entities;
using FoodstoreApi.Usecase.DTOs.Organization;
using FoodstoreApi.Usecase.Interfaces;
using System.Text.Json;

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
        ValidateBranchSettings(request.OpeningHoursJson, request.TaxSettingsJson, request.KitchenRouting);
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
            Address = request.Address?.Trim(), City = request.City?.Trim(), Phone = request.Phone?.Trim(),
            OpeningHoursJson = request.OpeningHoursJson?.Trim(), TaxSettingsJson = request.TaxSettingsJson?.Trim(),
            KitchenRouting = request.KitchenRouting?.Trim()
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

    public async Task<BranchDto?> UpdateBranchAsync(Guid organizationId, Guid branchId, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Branch name is required.");
        ValidateBranchSettings(request.OpeningHoursJson, request.TaxSettingsJson, request.KitchenRouting);

        var branch = await repository.GetBranchByIdAsync(branchId, cancellationToken);
        if (branch is null || branch.OrganizationId != organizationId)
            return null;

        branch.Name = request.Name.Trim();
        branch.Address = request.Address?.Trim();
        branch.City = request.City?.Trim();
        branch.Phone = request.Phone?.Trim();
        branch.OpeningHoursJson = request.OpeningHoursJson?.Trim();
        branch.TaxSettingsJson = request.TaxSettingsJson?.Trim();
        branch.KitchenRouting = request.KitchenRouting?.Trim();
        branch.IsActive = request.IsActive;
        return Map(await repository.UpdateBranchAsync(branch, cancellationToken));
    }

    public async Task<BranchDto?> AddBranchAsync(Guid organizationId, CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        ValidateNameAndCode(request.Name, request.Code, request.Name, request.Code);
        ValidateBranchSettings(request.OpeningHoursJson, request.TaxSettingsJson, request.KitchenRouting);
        var organization = await repository.GetByIdAsync(organizationId, cancellationToken);
        if (organization is null) return null;
        var normalizedCode = request.Code.Trim().ToUpperInvariant();
        if (organization.Branches.Any(e => e.Code.Equals(normalizedCode, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Branch code already exists for this organization.");

        var branch = await repository.AddBranchAsync(new Branch
        {
            Id = Guid.NewGuid(), OrganizationId = organizationId, Name = request.Name.Trim(), Code = normalizedCode,
            Address = request.Address?.Trim(), City = request.City?.Trim(), Phone = request.Phone?.Trim(),
            OpeningHoursJson = request.OpeningHoursJson?.Trim(), TaxSettingsJson = request.TaxSettingsJson?.Trim(),
            KitchenRouting = request.KitchenRouting?.Trim()
        }, cancellationToken);
        return Map(branch);
    }

    public async Task<IReadOnlyList<OrganizationMembershipDto>> GetMembershipsAsync(Guid organizationId, CancellationToken cancellationToken = default) =>
        (await repository.GetMembershipsAsync(organizationId, cancellationToken)).Select(MapMembership).ToList();

    public async Task<OrganizationMembershipDto?> UpdateMembershipAsync(Guid organizationId, Guid membershipId, UpdateOrganizationMembershipRequest request, CancellationToken cancellationToken = default)
    {
        var role = request.Role.Trim().ToLowerInvariant();
        if (role is not ("owner" or "manager" or "finance" or "member"))
            throw new InvalidOperationException("Organization role must be owner, manager, finance, or member.");
        var memberships = await repository.GetMembershipsAsync(organizationId, cancellationToken);
        var current = memberships.SingleOrDefault(item => item.Id == membershipId);
        if (current is null) return null;
        if (current.IsActive && current.Role == "owner" && (!request.IsActive || role != "owner") &&
            memberships.Count(item => item.IsActive && item.Role == "owner") == 1)
            throw new InvalidOperationException("An organization must keep at least one active owner.");
        var membership = await repository.UpdateMembershipAsync(organizationId, membershipId, role, request.IsActive, cancellationToken);
        return membership is null ? null : MapMembership(membership);
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

    private static void ValidateBranchSettings(string? openingHoursJson, string? taxSettingsJson, string? kitchenRouting)
    {
        ValidateJsonObject(openingHoursJson, "Opening hours");
        if (!string.IsNullOrWhiteSpace(taxSettingsJson))
        {
            using var tax = ParseJsonObject(taxSettingsJson, "Tax settings");
            if (tax.RootElement.TryGetProperty("vatRate", out var rate) &&
                (!rate.TryGetDecimal(out var value) || value is < 0m or > 100m))
                throw new InvalidOperationException("Tax vatRate must be a number from 0 to 100.");
        }
        if (kitchenRouting?.Trim().Length > 100)
            throw new InvalidOperationException("Kitchen routing must be 100 characters or fewer.");
    }

    private static void ValidateJsonObject(string? json, string label)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        using var _ = ParseJsonObject(json, label);
    }

    private static JsonDocument ParseJsonObject(string json, string label)
    {
        try
        {
            var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                document.Dispose();
                throw new InvalidOperationException($"{label} must be a JSON object.");
            }
            return document;
        }
        catch (JsonException)
        {
            throw new InvalidOperationException($"{label} contains invalid JSON.");
        }
    }

    private static OrganizationDto Map(Organization organization) => new(
        organization.Id, organization.Name, organization.Slug, organization.LegalName, organization.TaxId,
        organization.CurrencyCode, organization.TimeZone, organization.IsActive,
        organization.Branches.OrderBy(e => e.Name).Select(Map).ToList());

    private static BranchDto Map(Branch branch) => new(branch.Id, branch.Name, branch.Code, branch.Address, branch.City, branch.Phone, branch.IsActive, branch.OpeningHoursJson, branch.TaxSettingsJson, branch.KitchenRouting);
    private static OrganizationMembershipDto MapMembership(OrganizationMembership item) => new(item.Id, item.UserId,
        string.IsNullOrWhiteSpace(item.User.Name) ? item.User.UserName ?? item.UserId.ToString() : item.User.Name,
        item.User.Email, item.Role, item.IsActive, item.User.Employee?.BranchId, item.User.Employee?.Branch?.Name);
}
