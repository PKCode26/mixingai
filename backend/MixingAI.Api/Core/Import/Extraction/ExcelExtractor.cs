using ClosedXML.Excel;

namespace MixingAI.Api.Core.Import.Extraction;

public sealed class ExcelExtractor : IDocumentExtractor
{
    public bool CanHandle(string mimeContentType) =>
        mimeContentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        or "application/vnd.ms-excel";

    public Task<ExtractionResult> ExtractAsync(string storagePath, CancellationToken cancellationToken)
    {
        try
        {
            using var workbook = new XLWorkbook(storagePath);
            var issues = new List<string>();
            var fields = new List<StagedFieldData>();
            var fullTextBuilder = new System.Text.StringBuilder();

            // Felder aus Dateiname extrahieren
            fields.AddRange(FieldPatternMatcher.ParseFilename(Path.GetFileName(storagePath)));

            foreach (var sheet in workbook.Worksheets)
            {
                var sheetName = sheet.Name;
                var usedRange = sheet.RangeUsed();
                if (usedRange is null) continue;

                fullTextBuilder.AppendLine($"=== Tabellenblatt: {sheetName} ===");

                var rows = usedRange.RowsUsed().ToList();

                // Zellinhalte als Staged-Rohfelder speichern (Sheet:Name,Zelle:A1)
                foreach (var row in rows)
                {
                    var rowValues = new List<string>();
                    foreach (var cell in row.CellsUsed())
                    {
                        var cellText = cell.GetString().Trim();
                        if (string.IsNullOrEmpty(cellText)) continue;

                        rowValues.Add(cellText);
                        var cellRef = $"Sheet:{sheetName},Zelle:{cell.Address}";

                        // Wenn Zelle links eine Bezeichnung enthält und Zelle rechts den Wert
                        var nextCell = sheet.Cell(cell.Address.RowNumber, cell.Address.ColumnNumber + 1);
                        var nextValue = nextCell.GetString().Trim();

                        if (!string.IsNullOrEmpty(nextValue))
                        {
                            // Versuche bekannte Feldnamen zu erkennen
                            var matched = FieldPatternMatcher.Match($"{cellText}: {nextValue}", cellRef);
                            if (matched.Count > 0)
                            {
                                fields.AddRange(matched);
                            }
                            else
                            {
                                // Als Rohfeld speichern: Key = Zelleninhalt, Value = Nachbarzelle
                                fields.Add(new StagedFieldData(
                                    SanitizeKey(cellText),
                                    nextValue,
                                    0.5f,
                                    cellRef));
                            }
                        }
                    }
                    if (rowValues.Count > 0)
                        fullTextBuilder.AppendLine(string.Join(" | ", rowValues));
                }

                // Volltext auch durch Pattern-Matcher jagen
                var sheetText = fullTextBuilder.ToString();
                var textFields = FieldPatternMatcher.Match(sheetText, $"Sheet:{sheetName}");
                foreach (var tf in textFields)
                {
                    // Nur hinzufügen wenn noch nicht aus Zell-Analyse vorhanden
                    if (!fields.Any(f => f.Key == tf.Key))
                        fields.Add(tf);
                }
            }

            fields.Add(new StagedFieldData("Excel_Blattanzahl",
                workbook.Worksheets.Count.ToString(), 1.0f, "System"));

            if (fields.Count <= 1)
                issues.Add("Wenige Felder erkannt. Bitte im Review manuell prüfen.");

            return Task.FromResult(new ExtractionResult(
                Success: true,
                ErrorMessage: null,
                RawText: fullTextBuilder.ToString(),
                Fields: fields,
                Issues: issues));
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ExtractionResult(
                Success: false,
                ErrorMessage: $"Excel-Extraktion fehlgeschlagen: {ex.Message}",
                RawText: string.Empty,
                Fields: [],
                Issues: []));
        }
    }

    private static string SanitizeKey(string label) =>
        System.Text.RegularExpressions.Regex.Replace(label.Trim(), @"[^A-Za-z0-9äöüÄÖÜß_]", "_")
              .Trim('_')[..Math.Min(label.Length, 100)];
}
