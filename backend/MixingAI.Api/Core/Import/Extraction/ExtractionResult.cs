namespace MixingAI.Api.Core.Import.Extraction;

public record StagedFieldData(
    string Key,
    string? Value,
    float? Confidence,
    string? SourceRef);

public record ExtractedImageData(
    int PageNumber,
    int Index,
    byte[] Data,
    string MimeType);

public record ExtractionResult(
    bool Success,
    string? ErrorMessage,
    string RawText,
    IReadOnlyList<StagedFieldData> Fields,
    IReadOnlyList<string> Issues,
    IReadOnlyList<ExtractedImageData> Images);
