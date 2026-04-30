using MixingAI.Api.Core.Ai;
using MixingAI.Api.Core.Contracts;
using MixingAI.Api.Core.Import;
using MixingAI.Api.Core.Import.Extraction;
using MixingAI.Api.Core.Ocr;
using MixingAI.Api.Core.Security;
using MixingAI.Api.Core.Services;
using MixingAI.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Core.Endpoints;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/imports");
        group.MapPost("", CreateAsync);
        group.MapGet("", ListAsync);
        group.MapGet("{id:guid}", GetAsync);
        group.MapGet("{id:guid}/staged", GetStagedFieldsAsync);
        group.MapGet("{id:guid}/issues", GetIssuesAsync);
        group.MapPost("{id:guid}/approve", ApproveAsync);
        group.MapPost("{id:guid}/reject", RejectAsync);
        group.MapPost("{id:guid}/rework", RequestReworkAsync);
        group.MapPatch("{runId:guid}/staged/{fieldId:guid}", ConfirmFieldAsync);
        group.MapGet("{id:guid}/images", GetImagesAsync);
        group.MapGet("{id:guid}/images/{imageId:guid}", ServeImageAsync);
        group.MapPost("{id:guid}/ocr", TriggerOcrAsync);
        group.MapGet("ocr/status", GetOcrStatusAsync);
        group.MapPost("{id:guid}/analyze", AnalyzeWithOllamaAsync);
        group.MapGet("ollama/status", GetOllamaStatusAsync);
        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateImportRunRequest req,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var doc = await db.Documents.FindAsync([req.DocumentId], ct);
        if (doc is null) return Results.NotFound("Dokument nicht gefunden.");
        if (doc.IsArchived) return Results.BadRequest("Archivierte Dokumente können nicht importiert werden.");

        var run = new ImportRun { DocumentId = req.DocumentId };
        run.SetCreated(user.UserId, DateTime.UtcNow);

        db.ImportRuns.Add(run);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/imports/{run.Id}", ToRunResponse(run, doc.OriginalFileName, 0, 0));
    }

    private static async Task<IResult> ListAsync(
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        ImportRunStatus? status = null,
        Guid? documentId = null,
        CancellationToken ct = default)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var query = db.ImportRuns.Include(r => r.Document).AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        if (documentId.HasValue)
            query = query.Where(r => r.DocumentId == documentId.Value);

        var runs = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ImportRunResponse(
                r.Id, r.DocumentId, r.Document.OriginalFileName, r.Status,
                r.OperatorNotes, r.ErrorMessage, r.ExtractedAtUtc, r.CreatedAtUtc,
                r.StagedFields.Count, r.ValidationIssues.Count))
            .ToListAsync(ct);

        return Results.Ok(runs);
    }

    private static async Task<IResult> GetAsync(
        Guid id, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var run = await db.ImportRuns.Include(r => r.Document)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return Results.NotFound();

        var fieldCount = await db.StagedFields.CountAsync(f => f.ImportRunId == id, ct);
        var issueCount = await db.ValidationIssues.CountAsync(i => i.ImportRunId == id, ct);

        return Results.Ok(ToRunResponse(run, run.Document.OriginalFileName, fieldCount, issueCount));
    }

    private static async Task<IResult> GetStagedFieldsAsync(
        Guid id, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        if (!await db.ImportRuns.AnyAsync(r => r.Id == id, ct)) return Results.NotFound();

        var fields = await db.StagedFields
            .Where(f => f.ImportRunId == id)
            .OrderBy(f => f.FieldKey)
            .Select(f => new StagedFieldResponse(
                f.Id, f.FieldKey, f.FieldValue, f.Confidence, f.SourceRef, f.IsConfirmed))
            .ToListAsync(ct);

        return Results.Ok(fields);
    }

    private static async Task<IResult> GetIssuesAsync(
        Guid id, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        if (!await db.ImportRuns.AnyAsync(r => r.Id == id, ct)) return Results.NotFound();

        var issues = await db.ValidationIssues
            .Where(i => i.ImportRunId == id)
            .OrderBy(i => i.Severity)
            .Select(i => new ValidationIssueResponse(i.Id, i.Severity.ToString(), i.FieldKey, i.Message))
            .ToListAsync(ct);

        return Results.Ok(issues);
    }

    private static async Task<IResult> ApproveAsync(
        Guid id, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var run = await db.ImportRuns.Include(r => r.Document).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return Results.NotFound();
        if (run.Status is not (ImportRunStatus.NeedsReview or ImportRunStatus.NeedsRework))
            return Results.BadRequest($"Status '{run.Status}' erlaubt keine Freigabe.");

        run.Approve(user.UserId, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Results.Ok(ToRunResponse(run, run.Document.OriginalFileName, 0, 0));
    }

    private static async Task<IResult> RejectAsync(
        Guid id, ReviewDecisionRequest req, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var run = await db.ImportRuns.Include(r => r.Document).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return Results.NotFound();
        if (run.Status is not (ImportRunStatus.NeedsReview or ImportRunStatus.NeedsRework))
            return Results.BadRequest($"Status '{run.Status}' erlaubt keine Ablehnung.");

        run.Reject(req.Notes, user.UserId, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Results.Ok(ToRunResponse(run, run.Document.OriginalFileName, 0, 0));
    }

    private static async Task<IResult> RequestReworkAsync(
        Guid id, ReviewDecisionRequest req, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var run = await db.ImportRuns.Include(r => r.Document).FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return Results.NotFound();

        run.RequestRework(req.Notes, user.UserId, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        return Results.Ok(ToRunResponse(run, run.Document.OriginalFileName, 0, 0));
    }

    private static async Task<IResult> ConfirmFieldAsync(
        Guid runId, Guid fieldId, ConfirmFieldRequest req, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var field = await db.StagedFields
            .FirstOrDefaultAsync(f => f.Id == fieldId && f.ImportRunId == runId, ct);
        if (field is null) return Results.NotFound();

        field.IsConfirmed = req.IsConfirmed;
        if (req.FieldValue is not null)
            field.FieldValue = req.FieldValue;

        await db.SaveChangesAsync(ct);

        return Results.Ok(new StagedFieldResponse(
            field.Id, field.FieldKey, field.FieldValue,
            field.Confidence, field.SourceRef, field.IsConfirmed));
    }

    private static async Task<IResult> GetImagesAsync(
        Guid id, HttpContext ctx, AppDbContext db,
        CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        if (!await db.ImportRuns.AnyAsync(r => r.Id == id, ct)) return Results.NotFound();

        var images = await db.ExtractedImages
            .Where(i => i.ImportRunId == id)
            .OrderBy(i => i.PageNumber).ThenBy(i => i.ImageIndex)
            .Select(i => new ExtractedImageResponse(i.Id, i.PageNumber, i.ImageIndex, i.MimeType, i.FileSizeBytes))
            .ToListAsync(ct);

        return Results.Ok(images);
    }

    private static async Task<IResult> ServeImageAsync(
        Guid id, Guid imageId, HttpContext ctx, AppDbContext db,
        StorageService storage, CurrentUserService currentUser, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var img = await db.ExtractedImages
            .FirstOrDefaultAsync(i => i.Id == imageId && i.ImportRunId == id, ct);
        if (img is null) return Results.NotFound();

        var stream = storage.OpenRead(img.StoragePath);
        return Results.File(stream, img.MimeType);
    }

    private static async Task<IResult> TriggerOcrAsync(
        Guid id, HttpContext ctx, AppDbContext db, StorageService storage,
        IOcrProvider ocr, IEnumerable<IDocumentExtractor> extractors,
        CurrentUserService currentUser, ILogger<ImportProcessor> logger, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        if (!ocr.IsAvailable)
            return Results.Problem(
                detail: "OCR-Provider ist nicht konfiguriert. Bitte 'Ocr:Provider' in den Einstellungen setzen.",
                statusCode: StatusCodes.Status503ServiceUnavailable);

        var run = await db.ImportRuns.Include(r => r.Document)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return Results.NotFound();

        var fullPath = storage.GetFullPath(run.Document.StoragePath);
        var ocrResult = await ocr.ProcessAsync(fullPath, ct);

        if (!ocrResult.Success)
            return Results.Problem(detail: ocrResult.ErrorMessage, statusCode: 500);

        // OCR-Text als neue Felder in Staging schreiben
        var fullText = string.Join("\n", ocrResult.Pages.Select(p => p.Text));
        var newFields = FieldPatternMatcher.Match(fullText, "OCR");

        // Vorhandene OCR-Felder ersetzen
        var existing = await db.StagedFields
            .Where(f => f.ImportRunId == id && f.SourceRef == "OCR")
            .ToListAsync(ct);
        db.StagedFields.RemoveRange(existing);

        // OCR-Volltext aktualisieren
        var rawTextField = await db.StagedFields
            .FirstOrDefaultAsync(f => f.ImportRunId == id && f.FieldKey == "_RawText_OCR", ct);
        if (rawTextField is not null)
            db.StagedFields.Remove(rawTextField);

        foreach (var field in newFields)
        {
            db.StagedFields.Add(new StagedField
            {
                ImportRunId = id,
                FieldKey    = field.Key,
                FieldValue  = field.Value,
                Confidence  = field.Confidence,
                SourceRef   = "OCR",
            });
        }

        db.StagedFields.Add(new StagedField
        {
            ImportRunId = id,
            FieldKey    = "_RawText_OCR",
            FieldValue  = fullText[..Math.Min(fullText.Length, 50_000)],
            Confidence  = 1.0f,
            SourceRef   = "OCR",
        });

        // Wenn Status Failed/Queued → NeedsReview setzen
        if (run.Status is ImportRunStatus.Failed or ImportRunStatus.Queued)
            run.SetNeedsReview(DateTime.UtcNow);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("OCR abgeschlossen für ImportRun {Id}: {Count} Felder erkannt.", id, newFields.Count);

        return Results.Ok(new { fieldsFound = newFields.Count, pagesProcessed = ocrResult.Pages.Count });
    }

    private static IResult GetOcrStatusAsync(IOcrProvider ocr) =>
        Results.Ok(new OcrStatusResponse(ocr.IsAvailable,
            ocr.IsAvailable ? null : "OCR-Provider nicht konfiguriert"));

    private static IResult GetOllamaStatusAsync(IOllamaService ollama) =>
        Results.Ok(new OllamaStatusResponse(
            ollama.IsAvailable,
            ollama.ModelName,
            ollama.IsAvailable ? null : "Ollama nicht konfiguriert"));

    private static async Task<IResult> AnalyzeWithOllamaAsync(
        Guid id, HttpContext ctx, AppDbContext db, IOllamaService ollama,
        CurrentUserService currentUser, ILogger<ImportProcessor> logger, CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        if (!ollama.IsAvailable)
            return Results.Problem(
                detail: "Ollama ist nicht konfiguriert.",
                statusCode: StatusCodes.Status503ServiceUnavailable);

        var run = await db.ImportRuns.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (run is null) return Results.NotFound();

        // Rohtext aus Staging holen (bevorzuge OCR-Text, fallback auf PDF-Text)
        var rawField = await db.StagedFields
            .Where(f => f.ImportRunId == id && (f.FieldKey == "_RawText_OCR" || f.FieldKey == "_RawText"))
            .OrderByDescending(f => f.FieldKey) // _RawText_OCR zuerst
            .FirstOrDefaultAsync(ct);

        if (rawField is null || string.IsNullOrWhiteSpace(rawField.FieldValue))
            return Results.BadRequest("Kein extrahierter Text vorhanden. Bitte erst Dokument importieren.");

        var result = await ollama.ExtractAsync(rawField.FieldValue, ct);

        if (!result.Success)
            return Results.Problem(detail: result.ErrorMessage, statusCode: 500);

        // Vorhandene KI-Felder für diesen Run ersetzen
        var existing = await db.StagedFields
            .Where(f => f.ImportRunId == id && f.SourceRef == "KI")
            .ToListAsync(ct);
        db.StagedFields.RemoveRange(existing);

        var added = new List<StagedField>();
        foreach (var (key, value) in result.Fields)
        {
            if (key.StartsWith('_')) continue;
            var field = new StagedField
            {
                ImportRunId = id,
                FieldKey    = key,
                FieldValue  = value,
                Confidence  = 0.75f,
                SourceRef   = "KI",
            };
            db.StagedFields.Add(field);
            added.Add(field);
        }

        if (run.Status is ImportRunStatus.Failed or ImportRunStatus.Queued)
            run.SetNeedsReview(DateTime.UtcNow);

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Ollama-Analyse für ImportRun {Id}: {Count} Felder.", id, added.Count);

        var responses = added
            .Select(f => new StagedFieldResponse(f.Id, f.FieldKey, f.FieldValue, f.Confidence, f.SourceRef, f.IsConfirmed))
            .ToList();

        return Results.Ok(new OllamaAnalysisResponse(true, null, added.Count, responses));
    }

    private static ImportRunResponse ToRunResponse(
        ImportRun run, string docName, int fieldCount, int issueCount) =>
        new(run.Id, run.DocumentId, docName, run.Status, run.OperatorNotes,
            run.ErrorMessage, run.ExtractedAtUtc, run.CreatedAtUtc, fieldCount, issueCount);
}
