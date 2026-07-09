namespace FoodstoreApi.Core.Entities;

/// <summary>Immutable audit row for financial report views and exports.</summary>
public sealed class ReportAccessLog : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid ActorId { get; set; }
    public string ReportType { get; set; } = null!;
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public bool IsExport { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
