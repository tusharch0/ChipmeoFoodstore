namespace FoodstoreApi.Usecase.DTOs.Source;

public class CreateSourceDto
{
    public string Name { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public bool IsActive { get; set; } = true;
}




