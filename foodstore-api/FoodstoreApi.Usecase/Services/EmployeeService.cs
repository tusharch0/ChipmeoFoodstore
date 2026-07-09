using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Identity;
using FoodstoreApi.Usecase.DTOs.Employee;
using FoodstoreApi.Usecase.Interfaces;
using FoodstoreApi.Usecase.Utils;
using Microsoft.AspNetCore.Identity;

namespace FoodstoreApi.Usecase.Services;

public class EmployeeService(
    IEmployeeRepository employeeRepository,
    UserManager<ApplicationUser> userManager,
    IOrganizationRepository organizationRepository,
    ITenantAccessService tenantAccessService,
    ITenantContext tenantContext) : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IOrganizationRepository _organizationRepository = organizationRepository;
    private readonly ITenantAccessService _tenantAccessService = tenantAccessService;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<IEnumerable<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        return employees.Select(MapToDto);
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        return employee == null ? null : MapToDto(employee);
    }

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var branchId = dto.BranchId ?? _tenantContext.BranchId
            ?? throw new InvalidOperationException("A branch assignment is required.");
        if (!await _tenantAccessService.CanAccessBranchAsync(branchId, cancellationToken))
            throw new UnauthorizedAccessException("The selected branch is outside the current organization.");
        var branch = await _organizationRepository.GetBranchByIdAsync(branchId, cancellationToken)
            ?? throw new InvalidOperationException("Branch was not found.");
        var normalizedUsername = UsernameHelper.Normalize(dto.Username);
        var existing = await _userManager.FindByNameAsync(normalizedUsername);
        if (existing != null)
            throw new Exception("Username already exists");

        var user = new ApplicationUser
        {
            UserName = normalizedUsername,
            Email = dto.Email ?? "",
            Name = dto.FullName,
            Banned = !dto.IsActive,
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
            throw new Exception(string.Join("; ", createResult.Errors.Select(e => e.Description)));

        var employee = new Employee
        {
            UserId = user.Id,
            EmployeeCode = $"EMP-{Guid.NewGuid():N}"[..12].ToUpperInvariant(),
            RoleId = dto.RoleId,
            BranchId = branchId,
            Phone = dto.Phone,
            AvatarUrl = dto.AvatarUrl,
            Status = dto.IsActive ? (short)1 : (short)0,

        };

        var created = await _employeeRepository.CreateAsync(employee, cancellationToken);
        await _organizationRepository.EnsureMembershipAsync(user.Id, branch.OrganizationId, "member", cancellationToken);
        created.Branch = branch;
        return MapToDto(created);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateEmployeeDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, cancellationToken);
        if (employee == null) return false;

        employee.User.Name = dto.FullName;
        employee.User.Email = dto.Email ?? "";
        employee.Phone = dto.Phone;
        employee.AvatarUrl = dto.AvatarUrl;
        employee.RoleId = dto.RoleId;
        if (dto.BranchId.HasValue && dto.BranchId != employee.BranchId)
        {
            if (!await _tenantAccessService.CanAccessBranchAsync(dto.BranchId.Value, cancellationToken))
                throw new UnauthorizedAccessException("The selected branch is outside the current organization.");
            var branch = await _organizationRepository.GetBranchByIdAsync(dto.BranchId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Branch was not found.");
            employee.BranchId = branch.Id;
            employee.Branch = branch;
            await _organizationRepository.EnsureMembershipAsync(employee.UserId, branch.OrganizationId, "member", cancellationToken);
        }
        employee.Status = dto.IsActive ? (short)1 : (short)0;
        employee.User.Banned = !dto.IsActive;

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(employee.User);
            var resetResult = await _userManager.ResetPasswordAsync(employee.User, token, dto.Password);
            if (!resetResult.Succeeded)
                throw new Exception("Password update failed");
        }

        await _userManager.UpdateAsync(employee.User);
        return await _employeeRepository.UpdateAsync(employee, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _employeeRepository.DeleteAsync(id, cancellationToken);
    }

    private static EmployeeDto MapToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FullName = employee.User.Name,
            Username = employee.User.UserName ?? "",
            Email = employee.User.Email,
            Phone = employee.Phone,
            AvatarUrl = employee.AvatarUrl,
            RoleId = employee.RoleId,
            RoleName = employee.Role?.Name ?? "",
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.Name,
            OrganizationId = employee.Branch?.OrganizationId,
            OrganizationName = employee.Branch?.Organization?.Name,
            IsActive = !employee.User.Banned,
            LastLogin = employee.LastLogin,
            CreatedAt = employee.CreatedAt,
            UpdatedAt = employee.UpdatedAt,
            CreatedBy = employee.CreatedBy,
            UpdatedBy = employee.UpdatedBy
        };
    }
}
