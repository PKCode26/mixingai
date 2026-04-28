namespace MixingAI.Api.Core.Ocr;

public record OcrResult(
    bool Success,
    string? ErrorMessage,
    IReadOnlyList<OcrPageResult> Pages);

public record OcrPageResult(
    int PageNumber,
    string Text);
