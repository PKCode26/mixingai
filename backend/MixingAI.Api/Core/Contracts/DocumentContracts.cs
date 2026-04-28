using MixingAI.Api.Core.Documents;

namespace MixingAI.Api.Core.Contracts;

public record DocumentResponse(
    Guid Id,
    string OriginalFileName,
    string DisplayName,
    string MimeContentType,
    long FileSizeBytes,
    string ContentHash,
    DocumentType DocumentType,
    bool IsArchived,
    DateTime? ArchivedAtUtc,
    DateTime CreatedAtUtc,
    Guid? CreatedByUserId);

public record DocumentDuplicateResponse(Guid ExistingDocumentId, string OriginalFileName);
