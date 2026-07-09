namespace FoodstoreApi.Core.Entities;

public partial class PaymentSetting : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public virtual Branch? Branch { get; set; }
    public string BankId { get; set; } = null!;
    public string BankAccount { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string? BankAccountName { get; set; }
    public string Template { get; set; } = "compact2";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
