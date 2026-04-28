namespace MixingAI.Api.Core.Ocr;

/// <summary>
/// Fallback wenn kein OCR-Provider konfiguriert ist.
/// Austauschbar durch TesseractOcrProvider, AzureDocumentIntelligenceProvider o.Ä.
/// Konfiguration: appsettings -> Ocr:Provider = "tesseract" | "azure" | "disabled"
/// </summary>
public sealed class DisabledOcrProvider : IOcrProvider
{
    public bool IsAvailable => false;

    public Task<OcrResult> ProcessAsync(string filePath, CancellationToken ct) =>
        Task.FromResult(new OcrResult(
            Success: false,
            ErrorMessage: "OCR-Provider ist nicht konfiguriert. Bitte 'Ocr:Provider' in den Einstellungen setzen.",
            Pages: []));
}
