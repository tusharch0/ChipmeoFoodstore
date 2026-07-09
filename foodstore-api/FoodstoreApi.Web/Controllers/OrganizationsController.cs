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
public class OrganizationsController(IOrganizationService service) : ControllerBase
{
    [HttpGet]
    [RequirePermission("organization.view")]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken) =>
        ApiResult.Success(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [RequirePermission("organization.view")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var organization = await service.GetByIdAsync(id, cancellationToken);
        return organization is null ? ApiResult.NotFound("Organization not found.") : ApiResult.Success(organization);
    }

    [HttpPost]
    [RequirePermission("organization.manage")]
    public async Task<IActionResult> Create(CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
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
}
