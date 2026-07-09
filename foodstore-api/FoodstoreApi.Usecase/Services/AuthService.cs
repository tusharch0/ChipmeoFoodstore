using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodstoreApi.Core.Configuration;
using FoodstoreApi.Core.Entities;
using FoodstoreApi.Core.Entities.Identity;
using FoodstoreApi.Usecase.DTOs.Auth;
using FoodstoreApi.Usecase.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FoodstoreApi.Usecase.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    SignInManager<ApplicationUser> signInManager,
    IEmployeeRepository employeeRepository,
    IOrganizationRepository organizationRepository,
    IOptions<JwtSettings> jwtOptions) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly IEmployeeRepository _employeeRepository = employeeRepository;
    private readonly IOrganizationRepository _organizationRepository = organizationRepository;
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user == null || user.Banned)
            return null;

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return null;

        var employee = await _employeeRepository.GetByUserIdAsync(user.Id, cancellationToken);
        if (employee == null || employee.Status != 1)
            return null;

        employee.LastLogin = DateTime.UtcNow;

        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        var role = await _roleManager.FindByIdAsync(employee.RoleId.ToString());
        var isPlatformOperator = string.Equals(role?.Name, "root", StringComparison.OrdinalIgnoreCase);
        if (!isPlatformOperator && employee.Branch?.Organization is { IsActive: true } organization)
        {
            var hasMembership = await _organizationRepository.HasActiveMembershipAsync(user.Id, organization.Id, cancellationToken);
            if (!hasMembership)
                return null;
        }

        var permissionClaims = role != null
            ? await _roleManager.GetClaimsAsync(role)
            : new List<Claim>();

        var permissions = permissionClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        var token = GenerateJwtToken(user, employee, permissions, isPlatformOperator);
        var expiresIn = _jwtSettings.ExpiryInHours * 3600;

        return new LoginResponse
        {
            Token = token,
            RefreshToken = null,
            ExpiresIn = expiresIn,
            User = new UserInfo
            {
                Id = user.Id,
                EmployeeId = employee.Id,
                Name = user.Name,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                EmployeeCode = employee.EmployeeCode,
                AvatarUrl = employee.AvatarUrl,
                RoleId = employee.RoleId,
                RoleName = role?.Name ?? "Unknown",
                DefaultRoute = role?.DefaultRoute,
                Permissions = permissions
            }
        };
    }

    public Task<TokenResponse?> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Refresh token not yet implemented");
    }

    public Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<UserDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var employee = await _employeeRepository.GetByUserIdAsync(userId, cancellationToken);

        var role = employee != null
            ? await _roleManager.FindByIdAsync(employee.RoleId.ToString())
            : null;

        var permissionClaims = role != null
            ? await _roleManager.GetClaimsAsync(role)
            : new List<Claim>();

        var permissions = permissionClaims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            Phone = employee?.Phone,
            AvatarUrl = employee?.AvatarUrl,
            RoleName = role?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy,
            Permissions = permissions
        };
    }

    public async Task<UserDto?> UpdateProfileAsync(Guid userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null) return null;

        var employee = await _employeeRepository.GetByUserIdAsync(userId, cancellationToken);
        if (employee == null) return null;

        if (!string.IsNullOrEmpty(dto.Name)) user.Name = dto.Name;
        if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
        if (!string.IsNullOrEmpty(dto.Phone)) employee.Phone = dto.Phone;
        if (!string.IsNullOrEmpty(dto.AvatarUrl)) employee.AvatarUrl = dto.AvatarUrl;

        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (string.IsNullOrEmpty(dto.CurrentPassword))
                throw new ArgumentException("Current password is required to change password");

            var passwordResult = await _signInManager.CheckPasswordSignInAsync(user, dto.CurrentPassword, false);
            if (!passwordResult.Succeeded)
                throw new ArgumentException("Current password is incorrect");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!resetResult.Succeeded)
                throw new ArgumentException("Password change failed");
        }


        await _userManager.UpdateAsync(user);
        await _employeeRepository.UpdateAsync(employee, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.UserName ?? "",
            Email = user.Email ?? "",
            Phone = employee.Phone,
            AvatarUrl = employee.AvatarUrl,
            RoleName = (await _roleManager.FindByIdAsync(employee.RoleId.ToString()))?.Name ?? "Unknown",
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            CreatedBy = user.CreatedBy,
            UpdatedBy = user.UpdatedBy
        };
    }

    private string GenerateJwtToken(ApplicationUser user, Employee employee, List<string> permissions, bool isPlatformOperator)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? ""),
            new("username", user.UserName ?? ""),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, employee.Role?.Name ?? "User"),
            new("EmployeeId", employee.Id.ToString()),
            new("RoleId", employee.RoleId.ToString()),
            new("is_platform_operator", isPlatformOperator.ToString()),
        };

        if (employee.Branch is not null)
        {
            claims.Add(new Claim("branch_id", employee.Branch.Id.ToString()));
            claims.Add(new Claim("organization_id", employee.Branch.OrganizationId.ToString()));
        }

        foreach (var permission in permissions)
            claims.Add(new Claim("permission", permission));

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiryInHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
