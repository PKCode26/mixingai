namespace MixingAI.Api.Core.Import.Extraction;

public interface IDocumentExtractor
{
    bool CanHandle(string mimeContentType);
    Task<ExtractionResult> ExtractAsync(string storagePath, CancellationToken cancellationToken);
}
