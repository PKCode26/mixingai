using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace MixingAI.Api.Core.Import.Extraction;

public sealed class PdfExtractor : IDocumentExtractor
{
    public bool CanHandle(string mimeContentType) =>
        mimeContentType is "application/pdf";

    public Task<ExtractionResult> ExtractAsync(string storagePath, CancellationToken cancellationToken)
    {
        try
        {
            using var pdf = PdfDocument.Open(storagePath);
            var issues = new List<string>();
            var fields = new List<StagedFieldData>();
            var images = new List<ExtractedImageData>();
            var fullTextBuilder = new System.Text.StringBuilder();

            fields.AddRange(FieldPatternMatcher.ParseFilename(Path.GetFileName(storagePath)));

            int pageCount = pdf.NumberOfPages;

            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                var page = pdf.GetPage(pageNum);

                var pageText = ExtractPageText(page);
                fullTextBuilder.AppendLine($"=== Seite {pageNum} ===");
                fullTextBuilder.AppendLine(pageText);

                var sourceRef = pageCount > 1 ? $"Seite:{pageNum}" : "Seite:1";
                fields.AddRange(FieldPatternMatcher.Match(pageText, sourceRef));

                ExtractPageImages(page, pageNum, images);
            }

            var rawText = fullTextBuilder.ToString();

            if (string.IsNullOrWhiteSpace(rawText.Replace("=== Seite", "").Trim()))
            {
                issues.Add("Kein Text extrahierbar — möglicherweise gescanntes PDF. OCR-Verarbeitung empfohlen.");
                fields.Add(new StagedFieldData(
                    "OCR_Hinweis",
                    "Kein Textlayer gefunden. Bitte OCR-Knopf verwenden oder manuell prüfen.",
                    null, "System"));
            }

            if (fields.Count == 0)
                issues.Add("Keine bekannten Felder erkannt. Bitte manuell im Review prüfen.");

            fields.Add(new StagedFieldData("PDF_Seitenanzahl", pageCount.ToString(), 1.0f, "System"));

            return Task.FromResult(new ExtractionResult(
                Success: true,
                ErrorMessage: null,
                RawText: rawText,
                Fields: fields,
                Issues: issues,
                Images: images));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ExtractionResult(
                Success: false,
                ErrorMessage: $"PDF-Extraktion fehlgeschlagen: {ex.Message}",
                RawText: string.Empty,
                Fields: [],
                Issues: [],
                Images: []));
        }
    }

    private static string ExtractPageText(Page page)
    {
        var words = page.GetWords().ToList();
        if (words.Count == 0) return string.Empty;

        var lineGroups = words
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
            .OrderByDescending(g => g.Key)
            .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

        return string.Join("\n", lineGroups);
    }

    private static void ExtractPageImages(Page page, int pageNum, List<ExtractedImageData> results)
    {
        int idx = 0;
        foreach (var img in page.GetImages())
        {
            try
            {
                var raw = img.RawBytes.ToArray();
                if (raw.Length < 500) continue; // skip tiny/marker images

                var mime = DetectMime(raw);
                if (mime is null) continue; // skip unknown formats

                results.Add(new ExtractedImageData(pageNum, idx++, raw, mime));
            }
            catch
            {
                // skip images that can't be read
            }
        }
    }

    private static string? DetectMime(byte[] data)
    {
        if (data.Length < 4) return null;

        // JPEG: FF D8 FF
        if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
            return "image/jpeg";

        // PNG: 89 50 4E 47
        if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            return "image/png";

        // GIF: 47 49 46
        if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
            return "image/gif";

        return null;
    }
}
