namespace MixingAI.Api.Core.Ai;

public interface IOllamaService
{
    bool IsAvailable { get; }
    string ModelName { get; }
    Task<OllamaExtractionResult> ExtractAsync(string documentText, CancellationToken ct);
}
