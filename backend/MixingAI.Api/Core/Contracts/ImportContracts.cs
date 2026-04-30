using MixingAI.Api.Core.Import;

namespace MixingAI.Api.Core.Contracts;

public record ImportRunResponse(
    Guid Id,
    Guid DocumentId,
    string DocumentName,
    ImportRunStatus Status,
    string? OperatorNotes,
    string? ErrorMessage,
    DateTime? ExtractedAtUtc,
    DateTime CreatedAtUtc,
    int StagedFieldCount,
    int ValidationIssueCount);

public record StagedFieldResponse(
    Guid Id,
    string FieldKey,
    string? FieldValue,
    float? Confidence,
    string? SourceRef,
    bool IsConfirmed);

public record ValidationIssueResponse(
    Guid Id,
    string Severity,
    string? FieldKey,
    string Message);

public record CreateImportRunRequest(Guid DocumentId);

public record ConfirmFieldRequest(bool IsConfirmed, string? FieldValue);

public record ReviewDecisionRequest(string? Notes);

public record ExtractedImageResponse(
    Guid Id,
    int PageNumber,
    int ImageIndex,
    string MimeType,
    long FileSizeBytes);

public record OcrStatusResponse(bool IsAvailable, string? Message);

public record OllamaStatusResponse(bool IsAvailable, string ModelName, string? Message);

public record OllamaAnalysisResponse(
    bool Success,
    string? ErrorMessage,
    int FieldsFound,
    IReadOnlyList<StagedFieldResponse> Fields);
