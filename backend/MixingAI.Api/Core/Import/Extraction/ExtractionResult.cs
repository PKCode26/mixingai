namespace MixingAI.Api.Core.Import.Extraction;

public record StagedFieldData(
    string Key,
    string? Value,
    float? Confidence,
    string? SourceRef);

public record ExtractionResult(
    bool Success,
    string? ErrorMessage,
    string RawText,
    IReadOnlyList<StagedFieldData> Fields,
    IReadOnlyList<string> Issues);
