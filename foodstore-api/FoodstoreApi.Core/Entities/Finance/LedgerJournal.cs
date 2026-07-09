namespace FoodstoreApi.Core.Entities.Finance;

/// <summary>Posted journals are append-only; corrections use a separate journal linked by <see cref="ReversalOfJournalId"/>.</summary>
public sealed class LedgerJournal : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? BranchId { get; set; }
    public string Currency { get; set; } = "KES";
    public string EntryType { get; set; } = null!;
    public string SourceType { get; set; } = null!;
    public Guid SourceId { get; set; }
    public string Description { get; set; } = null!;
    public DateTime PostedAt { get; set; }
    public Guid? ReversalOfJournalId { get; set; }
    public Guid? ActorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public ICollection<LedgerJournalLine> Lines { get; set; } = new List<LedgerJournalLine>();
}

public sealed class LedgerJournalLine : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid JournalId { get; set; }
    public Guid AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Memo { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public LedgerJournal Journal { get; set; } = null!;
    public LedgerAccount Account { get; set; } = null!;
}
