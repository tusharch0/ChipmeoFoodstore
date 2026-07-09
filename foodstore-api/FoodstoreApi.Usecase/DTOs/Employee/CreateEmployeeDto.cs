namespace FoodstoreApi.Usecase.DTOs.Employee;

public class CreateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid RoleId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid RoleId { get; set; }
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; }
    public string? Password { get; set; } // Optional for update
}




