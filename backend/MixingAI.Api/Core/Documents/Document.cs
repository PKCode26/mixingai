namespace MixingAI.Api.Core.Documents;

public sealed class Document : AuditableEntity
{
    public required string OriginalFileName { get; init; }
    public string DisplayName { get; private set; } = null!;
    public string MimeContentType { get; private set; } = null!;
    public long FileSizeBytes { get; private set; }
    public string ContentHash { get; private set; } = null!;
    public string StoragePath { get; private set; } = null!;
    public DocumentType DocumentType { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTime? ArchivedAtUtc { get; private set; }
    public Guid? ArchivedByUserId { get; private set; }

    public void Initialize(string displayName, string mimeContentType, long fileSizeBytes,
        string contentHash, string storagePath, DocumentType documentType)
    {
        DisplayName = displayName;
        MimeContentType = mimeContentType;
        FileSizeBytes = fileSizeBytes;
        ContentHash = contentHash;
        StoragePath = storagePath;
        DocumentType = documentType;
    }

    public void Archive(Guid byUserId, DateTime utcNow)
    {
        IsArchived = true;
        ArchivedAtUtc = utcNow;
        ArchivedByUserId = byUserId;
    }

    public void Unarchive()
    {
        IsArchived = false;
        ArchivedAtUtc = null;
        ArchivedByUserId = null;
    }
}
