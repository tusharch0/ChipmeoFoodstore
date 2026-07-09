using FoodstoreApi.Usecase.DTOs.Organization;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Web.ApiResponse;
using FoodstoreApi.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodstoreApi.Web.Controllers;

[ApiController]
[Route("api/admin/organizations")]
[Authorize]
public class OrganizationsController(IOrganizationService service, ITenantAccessService tenantAccess, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    [RequirePermission("organization.view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var organizations = await service.GetAllAsync(cancellationToken);
        if (tenantContext.IsPlatformOperator || tenantContext.IsSystem)
            return ApiResult.Success(organizations);

        var scoped = organizations.Where(o => o.Id == tenantContext.OrganizationId)
            .Select(o => tenantContext.IsOrganizationOwner ? o : o with
            {
                Branches = o.Branches.Where(b => b.Id == tenantContext.BranchId).ToList()
            }).ToList();
        return ApiResult.Success(scoped);
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("organization.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        if (!await tenantAccess.CanAccessOrganizationAsync(id, cancellationToken)) return Forbid();
        var organization = await service.GetByIdAsync(id, cancellationToken);
        return organization is null ? ApiResult.NotFound("Organization not found.") : ApiResult.Success(organization);
    }

    [HttpPost]
    [RequirePermission("organization.manage")]
    public async Task<IActionResult> Create(CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        if (!tenantContext.IsPlatformOperator && !tenantContext.IsSystem) return Forbid();
        try
        {
            var created = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException exception)
        {
            return ApiResult.BadRequest(exception.Message);
        }
    }

    [HttpPost("{organizationId:guid}/branches")]
    [RequirePermission("organization.manage")]
    public async Task<IActionResult> AddBranch(Guid organizationId, CreateBranchRequest request, CancellationToken cancellationToken)
    {
        if (!await tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken)) return Forbid();
        try
        {
            var branch = await service.AddBranchAsync(organizationId, request, cancellationToken);
            return branch is null ? ApiResult.NotFound("Organization not found.") : ApiResult.Success(branch);
        }
        catch (InvalidOperationException exception)
        {
            return ApiResult.BadRequest(exception.Message);
        }
    }

    [HttpPut("{organizationId:guid}/branches/{branchId:guid}")]
    [RequirePermission("organization.manage")]
    public async Task<IActionResult> UpdateBranch(Guid organizationId, Guid branchId, UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        if (!await tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken)) return Forbid();
        try
        {
            var branch = await service.UpdateBranchAsync(organizationId, branchId, request, cancellationToken);
            return branch is null ? ApiResult.NotFound("Branch not found.") : ApiResult.Success(branch);
        }
        catch (InvalidOperationException exception)
        {
            return ApiResult.BadRequest(exception.Message);
        }
    }


    [HttpGet("{organizationId:guid}/memberships")]
    [RequirePermission("organization.manage")]
    public async Task<IActionResult> GetMemberships(Guid organizationId, CancellationToken cancellationToken)
    {
        if (!CanManageMemberships(organizationId) || !await tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken)) return Forbid();
        return ApiResult.Success(await service.GetMembershipsAsync(organizationId, cancellationToken));
    }

    [HttpPut("{organizationId:guid}/memberships/{membershipId:guid}")]
    [RequirePermission("organization.manage")]
    public async Task<IActionResult> UpdateMembership(Guid organizationId, Guid membershipId, UpdateOrganizationMembershipRequest request, CancellationToken cancellationToken)
    {
        if (!CanManageMemberships(organizationId) || !await tenantAccess.CanAccessOrganizationAsync(organizationId, cancellationToken)) return Forbid();
        try
        {
            var membership = await service.UpdateMembershipAsync(organizationId, membershipId, request, cancellationToken);
            return membership is null ? ApiResult.NotFound("Membership not found.") : ApiResult.Success(membership);
        }
        catch (InvalidOperationException exception) { return ApiResult.BadRequest(exception.Message); }
    }

    private bool CanManageMemberships(Guid organizationId) => tenantContext.IsPlatformOperator || tenantContext.IsSystem ||
        (tenantContext.IsOrganizationOwner && tenantContext.OrganizationId == organizationId);
}
