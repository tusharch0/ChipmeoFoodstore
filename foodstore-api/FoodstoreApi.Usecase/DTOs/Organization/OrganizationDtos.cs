namespace FoodstoreApi.Usecase.DTOs.Organization;

public sealed record CreateOrganizationRequest(
    string Name,
    string Slug,
    string PrimaryBranchName,
    string PrimaryBranchCode,
    string? LegalName = null,
    string? TaxId = null,
    string? Address = null,
    string? City = null,
    string? Phone = null,
    string? OpeningHoursJson = null,
    string? TaxSettingsJson = null,
    string? KitchenRouting = null,
    Guid? OwnerUserId = null);

public sealed record CreateBranchRequest(
    string Name,
    string Code,
    string? Address = null,
    string? City = null,
    string? Phone = null,
    string? OpeningHoursJson = null,
    string? TaxSettingsJson = null,
    string? KitchenRouting = null);

public sealed record UpdateBranchRequest(
    string Name,
    string? Address = null,
    string? City = null,
    string? Phone = null,
    string? OpeningHoursJson = null,
    string? TaxSettingsJson = null,
    string? KitchenRouting = null,
    bool IsActive = true);

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string Slug,
    string? LegalName,
    string? TaxId,
    string CurrencyCode,
    string TimeZone,
    bool IsActive,
    IReadOnlyList<BranchDto> Branches);

public sealed record BranchDto(
    Guid Id,
    string Name,
    string Code,
    string? Address,
    string? City,
    string? Phone,
    bool IsActive,
    string? OpeningHoursJson,
    string? TaxSettingsJson,
    string? KitchenRouting);

public sealed record OrganizationMembershipDto(Guid Id, Guid UserId, string Name, string? Email, string Role, bool IsActive, Guid? BranchId, string? BranchName);
public sealed record UpdateOrganizationMembershipRequest(string Role, bool IsActive = true);
