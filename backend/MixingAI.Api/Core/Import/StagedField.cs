namespace MixingAI.Api.Core.Import;

public sealed class StagedField
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ImportRunId { get; init; }
    public ImportRun ImportRun { get; init; } = null!;

    // e.g. "Kunde", "Versuchsnummer", "Gesamtmenge", "Rohstoff[0].Name"
    public required string FieldKey { get; init; }
    public string? FieldValue { get; set; }

    // 0.0 – 1.0, null = determined by rule (high confidence)
    public float? Confidence { get; init; }

    // e.g. "Seite:1", "Sheet:Rezept,Zelle:B5", "Dateiname"
    public string? SourceRef { get; init; }

    // Whether this field was reviewed/confirmed by a user
    public bool IsConfirmed { get; set; }
}
