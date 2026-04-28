using MixingAI.Api.Core.Documents;

namespace MixingAI.Api.Core.Import;

public sealed class ImportRun : AuditableEntity
{
    public Guid DocumentId { get; init; }
    public Document Document { get; init; } = null!;

    public ImportRunStatus Status { get; private set; } = ImportRunStatus.Queued;
    public string? OperatorNotes { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? ExtractedAtUtc { get; private set; }

    public ICollection<StagedField> StagedFields { get; init; } = [];
    public ICollection<ValidationIssue> ValidationIssues { get; init; } = [];

    public void SetExtracting() => Status = ImportRunStatus.Extracting;

    public void SetNeedsReview(DateTime utcNow)
    {
        Status = ImportRunStatus.NeedsReview;
        ExtractedAtUtc = utcNow;
        ErrorMessage = null;
    }

    public void SetFailed(string errorMessage)
    {
        Status = ImportRunStatus.Failed;
        ErrorMessage = errorMessage;
    }

    public void Approve(Guid byUserId, DateTime utcNow)
    {
        Status = ImportRunStatus.Approved;
        SetUpdated(byUserId, utcNow);
    }

    public void Reject(string? notes, Guid byUserId, DateTime utcNow)
    {
        Status = ImportRunStatus.Rejected;
        OperatorNotes = notes;
        SetUpdated(byUserId, utcNow);
    }

    public void RequestRework(string? notes, Guid byUserId, DateTime utcNow)
    {
        Status = ImportRunStatus.NeedsRework;
        OperatorNotes = notes;
        SetUpdated(byUserId, utcNow);
    }
}
