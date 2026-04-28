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
            var fullTextBuilder = new System.Text.StringBuilder();

            // Felder aus Dateiname extrahieren (höchste Zuverlässigkeit)
            var filenameFields = FieldPatternMatcher.ParseFilename(Path.GetFileName(storagePath));
            fields.AddRange(filenameFields);

            int pageCount = pdf.NumberOfPages;

            // Pro Seite Text extrahieren (1 Seite = 1 Versuch laut Protokollstruktur)
            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                var page = pdf.GetPage(pageNum);
                var pageText = ExtractPageText(page);
                fullTextBuilder.AppendLine($"=== Seite {pageNum} ===");
                fullTextBuilder.AppendLine(pageText);

                var sourceRef = pageCount > 1 ? $"Seite:{pageNum}" : "Seite:1";
                var pageFields = FieldPatternMatcher.Match(pageText, sourceRef);
                fields.AddRange(pageFields);
            }

            var rawText = fullTextBuilder.ToString();

            if (string.IsNullOrWhiteSpace(rawText.Replace("=== Seite", "").Trim()))
            {
                issues.Add("Kein Text extrahierbar — möglicherweise gescanntes PDF. OCR-Verarbeitung empfohlen.");
                // Rohtexteintrag damit der Reviewer sieht was ankam
                fields.Add(new StagedFieldData(
                    "OCR_Hinweis",
                    "Kein Textlayer gefunden. Bitte manuell prüfen oder OCR aktivieren.",
                    null, "System"));
            }

            if (fields.Count == 0)
                issues.Add("Keine bekannten Felder erkannt. Bitte manuell im Review prüfen.");

            // Seitenanzahl als Metafeld
            fields.Add(new StagedFieldData("PDF_Seitenanzahl", pageCount.ToString(), 1.0f, "System"));

            return Task.FromResult(new ExtractionResult(
                Success: true,
                ErrorMessage: null,
                RawText: rawText,
                Fields: fields,
                Issues: issues));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ExtractionResult(
                Success: false,
                ErrorMessage: $"PDF-Extraktion fehlgeschlagen: {ex.Message}",
                RawText: string.Empty,
                Fields: [],
                Issues: []));
        }
    }

    private static string ExtractPageText(Page page)
    {
        // Wörter nach Y-Position (Zeile) und X-Position gruppieren
        var words = page.GetWords().ToList();
        if (words.Count == 0) return string.Empty;

        var lineGroups = words
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 0))
            .OrderByDescending(g => g.Key)
            .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text)));

        return string.Join("\n", lineGroups);
    }
}
