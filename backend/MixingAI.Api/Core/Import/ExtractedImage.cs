namespace MixingAI.Api.Core.Import;

public sealed class ExtractedImage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ImportRunId { get; init; }
    public ImportRun ImportRun { get; init; } = null!;

    public int PageNumber { get; init; }
    public int ImageIndex { get; init; }

    // relative path under StorageService root
    public required string StoragePath { get; init; }
    public string MimeType { get; init; } = "image/jpeg";
    public long FileSizeBytes { get; init; }
}
