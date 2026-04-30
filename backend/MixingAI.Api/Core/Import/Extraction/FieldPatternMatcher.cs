using System.Text.RegularExpressions;

namespace MixingAI.Api.Core.Import.Extraction;

/// <summary>
/// Erkennt Felder aus Versuchsprotokoll-Text anhand bekannter Feldnamen.
/// Quelle: amixon-Versuchsprotokoll-Struktur (1 Seite pro Versuch).
/// </summary>
public static partial class FieldPatternMatcher
{
    public static readonly IReadOnlySet<string> RequiredFieldKeys = new HashSet<string>(StringComparer.Ordinal)
    {
        "Datum",
        "Versuchsnummer",
        "Kunde",
        "Mischzeit",
        "Gesamtmenge",
    };

    // Feldname → Key (sortiert nach Priorität/Spezifität)
    private static readonly (string Pattern, string Key, float Confidence)[] Rules =
    [
        // === Dokumentkopf ===
        (@"Kunde[:\s]+([^\n\r]+)",                                              "Kunde",                    0.85f),
        (@"Versuchsteilnehmer\s+Kundenseits[:\s]+([^\n\r]+)",                   "TeilnehmerKunde",          0.85f),
        (@"Versuchsteilnehmer\s+amixon[:\s]+([^\n\r]+)",                        "TeilnehmerAmixon",         0.85f),
        (@"Aufgabenstellung[:\s]+([^\n\r]+)",                                   "Aufgabenstellung",         0.80f),
        (@"Testapparat[:\s]+([^\n\r]+)",                                        "Testapparat",              0.85f),

        // === Versuchsvorbereitung ===
        (@"Versuch(?:snummer)?[:\s#]+([A-Z0-9\-\/\.]+)",                        "Versuchsnummer",           0.90f),
        (@"Produkt[:\s]+([^\n\r]+)",                                            "Produkt",                  0.85f),
        (@"Ziel\s+des\s+Versuchs[:\s]+([^\n\r]+)",                             "Versuchsziel",             0.85f),

        // === Versuchsdurchführung ===
        (@"Versuchsaggregat[:\s]+([^\n\r]+)",                                   "Versuchsaggregat",         0.90f),
        (@"Sonderausstattung[:\s]+([^\n\r]+)",                                  "Sonderausstattung",        0.80f),
        (@"Bef[üu]llen?\s+mit[:\s]+([^\n\r]+)",                                "BefuellenMit",             0.85f),
        (@"Gesamtmenge[:\s]+([^\n\r,]+(?:kg|g|l|L|t)?)",                       "Gesamtmenge",              0.85f),

        // === Prozessparameter (oft tabellarisch, auch als Freitext) ===
        (@"Mischzeit[:\s]+(\d+(?:[,\.]\d+)?)\s*(?:min|h|s|Sek\.?)?",          "Mischzeit",                0.80f),
        (@"(?:Drehzahl|n\s*=|RPM)[:\s]+(\d+(?:[,\.]\d+)?)\s*(?:rpm|U/min|min-1)?", "Drehzahl",            0.75f),
        (@"Temperatur[:\s]+(\d+(?:[,\.]\d+)?)\s*°?C",                          "Temperatur",               0.80f),
        (@"(?:Bef[üu]llgrad|F[üu]llgrad)[:\s]+(\d+(?:[,\.]\d+)?)\s*%",        "Fuellgrad",                0.75f),
        (@"(?:Chargengewicht|Chargenm\.?)[:\s]+([^\n\r]+)",                    "Chargengewicht",           0.75f),

        // === Datum / Zeitstempel ===
        (@"Datum[:\s]+(\d{1,2}[\./\-]\d{1,2}[\./\-]\d{2,4})",                 "Datum",                    0.90f),
    ];

    /// <summary>
    /// Wendet alle Muster auf einen Textblock an und gibt erkannte Felder zurück.
    /// </summary>
    public static List<StagedFieldData> Match(string text, string sourceRef)
    {
        var results = new List<StagedFieldData>();
        foreach (var (pattern, key, confidence) in Rules)
        {
            var match = Regex.Match(text, pattern,
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.CultureInvariant);
            if (!match.Success) continue;

            var value = match.Groups[1].Value.Trim();
            if (!string.IsNullOrWhiteSpace(value))
                results.Add(new StagedFieldData(key, value, confidence, sourceRef));
        }
        return results;
    }

    /// <summary>
    /// Versucht, Felder aus dem Dateinamen zu extrahieren.
    /// Erwartetes Format: "{Versuchsnummer} {Kunde} {Mischertyp} {Baugröße}-{Fabrikatnummer}"
    /// </summary>
    public static List<StagedFieldData> ParseFilename(string fileName)
    {
        var results = new List<StagedFieldData>();
        var name = Path.GetFileNameWithoutExtension(fileName).Trim();

        // Letztes Token vor dem letzten Leerzeichen = "Baugröße-Fabrikatnummer"
        var lastSpaceIdx = name.LastIndexOf(' ');
        if (lastSpaceIdx <= 0) return results;

        var machineToken = name[lastSpaceIdx..].Trim();   // z.B. "AMK5-12345"
        var rest = name[..lastSpaceIdx].Trim();

        // Maschine: optional Bindestrich trennt Baugröße von Fabrikatnummer
        var dashIdx = machineToken.IndexOf('-');
        if (dashIdx > 0)
        {
            results.Add(new StagedFieldData("Baugroesse",      machineToken[..dashIdx],   1.0f, "Dateiname"));
            results.Add(new StagedFieldData("Fabrikatnummer",  machineToken[(dashIdx+1)..], 1.0f, "Dateiname"));
        }
        else
        {
            results.Add(new StagedFieldData("Versuchsaggregat", machineToken, 0.7f, "Dateiname"));
        }

        // Zweitletztes Token = Mischertyp (z.B. "AMK", "KoneSlid")
        var secondLastSpace = rest.LastIndexOf(' ');
        if (secondLastSpace > 0)
        {
            results.Add(new StagedFieldData("Mischertyp", rest[(secondLastSpace+1)..].Trim(), 0.9f, "Dateiname"));
            rest = rest[..secondLastSpace].Trim();
        }

        // Erstes Token = Versuchsnummer (enthält keine Leerzeichen)
        var firstSpace = rest.IndexOf(' ');
        if (firstSpace > 0)
        {
            results.Add(new StagedFieldData("Versuchsnummer", rest[..firstSpace].Trim(), 1.0f, "Dateiname"));
            // Rest dazwischen = Kunde (kann Leerzeichen enthalten)
            results.Add(new StagedFieldData("Kunde", rest[(firstSpace+1)..].Trim(), 0.9f, "Dateiname"));
        }
        else
        {
            results.Add(new StagedFieldData("Versuchsnummer", rest, 1.0f, "Dateiname"));
        }

        return results;
    }
}
