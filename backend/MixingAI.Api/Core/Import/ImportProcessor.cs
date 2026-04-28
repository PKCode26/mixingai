using MixingAI.Api.Core.Import.Extraction;
using MixingAI.Api.Core.Services;
using MixingAI.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Core.Import;

public sealed class ImportProcessor(
    IServiceScopeFactory scopeFactory,
    IEnumerable<IDocumentExtractor> extractors,
    StorageService storage,
    ILogger<ImportProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ImportProcessor gestartet.");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueuedRunsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Fehler im ImportProcessor-Loop.");
            }
            await Task.Delay(TimeSpan.FromSeconds(4), stoppingToken);
        }
    }

    private async Task ProcessQueuedRunsAsync(CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var queued = await db.ImportRuns
            .Include(r => r.Document)
            .Where(r => r.Status == ImportRunStatus.Queued)
            .OrderBy(r => r.CreatedAtUtc)
            .Take(5)
            .ToListAsync(ct);

        foreach (var run in queued)
        {
            await ProcessRunAsync(db, run, ct);
        }
    }

    private async Task ProcessRunAsync(AppDbContext db, ImportRun run, CancellationToken ct)
    {
        logger.LogInformation("Verarbeite ImportRun {Id} (Dokument: {File})",
            run.Id, run.Document.OriginalFileName);

        run.SetExtracting();
        await db.SaveChangesAsync(ct);

        var fullPath = storage.GetFullPath(run.Document.StoragePath);
        var extractor = extractors.FirstOrDefault(e => e.CanHandle(run.Document.MimeContentType));

        ExtractionResult result;
        if (extractor is null)
        {
            result = new ExtractionResult(
                Success: false,
                ErrorMessage: $"Kein Extraktor für MIME-Typ '{run.Document.MimeContentType}' verfügbar.",
                RawText: string.Empty,
                Fields: [],
                Issues: []);
        }
        else
        {
            result = await extractor.ExtractAsync(fullPath, ct);
        }

        if (!result.Success)
        {
            run.SetFailed(result.ErrorMessage ?? "Unbekannter Fehler");
            await db.SaveChangesAsync(ct);
            logger.LogWarning("ImportRun {Id} fehlgeschlagen: {Error}", run.Id, result.ErrorMessage);
            return;
        }

        // Staged Fields persistieren
        var now = DateTime.UtcNow;
        foreach (var field in result.Fields)
        {
            db.StagedFields.Add(new StagedField
            {
                ImportRunId = run.Id,
                FieldKey    = field.Key,
                FieldValue  = field.Value,
                Confidence  = field.Confidence,
                SourceRef   = field.SourceRef,
            });
        }

        // Validation Issues persistieren
        foreach (var issue in result.Issues)
        {
            db.ValidationIssues.Add(new ValidationIssue
            {
                ImportRunId = run.Id,
                Severity    = IssueSeverity.Warning,
                Message     = issue,
            });
        }

        // Rohtexte als Sonderfeld (für spätere Suche/Review)
        if (!string.IsNullOrWhiteSpace(result.RawText))
        {
            db.StagedFields.Add(new StagedField
            {
                ImportRunId = run.Id,
                FieldKey    = "_RawText",
                FieldValue  = result.RawText[..Math.Min(result.RawText.Length, 50_000)],
                Confidence  = 1.0f,
                SourceRef   = "System",
            });
        }

        run.SetNeedsReview(now);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("ImportRun {Id}: {Count} Felder extrahiert, Status NeedsReview.",
            run.Id, result.Fields.Count);
    }
}
