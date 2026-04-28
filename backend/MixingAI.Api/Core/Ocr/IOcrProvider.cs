namespace MixingAI.Api.Core.Ocr;

public interface IOcrProvider
{
    bool IsAvailable { get; }
    Task<OcrResult> ProcessAsync(string filePath, CancellationToken ct);
}
