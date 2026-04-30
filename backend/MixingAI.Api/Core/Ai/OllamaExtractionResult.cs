namespace MixingAI.Api.Core.Ai;

public record OllamaExtractionResult(
    bool Success,
    string? ErrorMessage,
    IReadOnlyDictionary<string, string> Fields,
    string? RawResponse);
