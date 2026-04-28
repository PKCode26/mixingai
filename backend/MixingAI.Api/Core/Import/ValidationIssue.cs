namespace MixingAI.Api.Core.Import;

public sealed class ValidationIssue
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ImportRunId { get; init; }
    public ImportRun ImportRun { get; init; } = null!;

    public IssueSeverity Severity { get; init; }
    public string? FieldKey { get; init; }
    public required string Message { get; init; }
}
