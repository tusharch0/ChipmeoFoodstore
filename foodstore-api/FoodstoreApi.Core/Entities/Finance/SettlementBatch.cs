namespace FoodstoreApi.Core.Entities.Finance;

/// <summary>Immutable settlement calculation for one organization, currency and period once approved.</summary>
public sealed class SettlementBatch : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Currency { get; set; } = "KES";
    public DateOnly PeriodDate { get; set; }
    public string Status { get; set; } = "draft";
    public decimal GrossSales { get; set; }
    public decimal Refunds { get; set; }
    public decimal ProviderFees { get; set; }
    public decimal Commissions { get; set; }
    public decimal Adjustments { get; set; }
    public decimal Holds { get; set; }
    public decimal NetAmount { get; set; }
    public string IdempotencyKey { get; set; } = null!;
    public string? PayoutReference { get; set; }
    public string? FailureReason { get; set; }
    public Guid? ReviewedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public Guid? PaidBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public ICollection<SettlementLine> Lines { get; set; } = new List<SettlementLine>();
}

public sealed class SettlementLine : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid SettlementBatchId { get; set; }
    public Guid PaymentIntentId { get; set; }
    public string? Provider { get; set; }
    public string? ProviderReference { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal ProviderFeeAmount { get; set; }
    public decimal NetAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public SettlementBatch SettlementBatch { get; set; } = null!;
}
