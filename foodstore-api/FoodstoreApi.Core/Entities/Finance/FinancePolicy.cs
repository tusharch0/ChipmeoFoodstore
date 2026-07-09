namespace FoodstoreApi.Core.Entities.Finance;

/// <summary>Per-restaurant financial policy. Values are stored as configured, never inferred from client input.</summary>
public sealed class FinancePolicy : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string CommissionMode { get; set; } = "percentage";
    public decimal CommissionValue { get; set; }
    public string ProviderFeeMode { get; set; } = "percentage";
    public decimal ProviderFeeValue { get; set; }
    public string FeeBearer { get; set; } = "restaurant";
    public string SettlementMode { get; set; } = "manual";
    public decimal ManualAdjustmentApprovalLimit { get; set; } = 100000m;
    public decimal MinimumPayoutThreshold { get; set; } = 1000m;
    public string SettlementCalendar { get; set; } = "daily";
    public string TaxTreatment { get; set; } = "exclusive";
    public decimal TaxRate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}

public sealed class FinanceAdjustmentRequest : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid LedgerAccountId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public string ReasonCode { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string Status { get; set; } = "pending";
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? JournalId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
