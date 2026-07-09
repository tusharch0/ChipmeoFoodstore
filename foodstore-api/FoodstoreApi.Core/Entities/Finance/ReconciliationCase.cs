namespace FoodstoreApi.Core.Entities.Finance;

/// <summary>Immutable investigation trail for provider or payout discrepancies.</summary>
public sealed class ReconciliationCase : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? SettlementBatchId { get; set; }
    public string SourceType { get; set; } = null!;
    public string SourceReference { get; set; } = null!;
    public decimal InternalAmount { get; set; }
    public decimal ExternalAmount { get; set; }
    public string Currency { get; set; } = "KES";
    public string Status { get; set; } = "open";
    public string? EvidenceUrl { get; set; }
    public string? Resolution { get; set; }
    public Guid? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
