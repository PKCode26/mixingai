using MixingAI.Api.Core.Contracts;
using MixingAI.Api.Core.Import;
using MixingAI.Api.Core.Security;
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

        var query = db.ImportRuns
            .Include(r => r.Document)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);
        if (documentId.HasValue)
            query = query.Where(r => r.DocumentId == documentId.Value);

        var runs = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ImportRunResponse(
                r.Id,
                r.DocumentId,
                r.Document.OriginalFileName,
                r.Status,
                r.OperatorNotes,
                r.ErrorMessage,
                r.ExtractedAtUtc,
                r.CreatedAtUtc,
                r.StagedFields.Count,
                r.ValidationIssues.Count))
            .ToListAsync(ct);

        return Results.Ok(runs);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var run = await db.ImportRuns
            .Include(r => r.Document)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

        if (run is null) return Results.NotFound();

        var fieldCount = await db.StagedFields.CountAsync(f => f.ImportRunId == id, ct);
        var issueCount = await db.ValidationIssues.CountAsync(i => i.ImportRunId == id, ct);

        return Results.Ok(ToRunResponse(run, run.Document.OriginalFileName, fieldCount, issueCount));
    }

    private static async Task<IResult> GetStagedFieldsAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var exists = await db.ImportRuns.AnyAsync(r => r.Id == id, ct);
        if (!exists) return Results.NotFound();

        var fields = await db.StagedFields
            .Where(f => f.ImportRunId == id)
            .OrderBy(f => f.FieldKey)
            .Select(f => new StagedFieldResponse(
                f.Id, f.FieldKey, f.FieldValue, f.Confidence, f.SourceRef, f.IsConfirmed))
            .ToListAsync(ct);

        return Results.Ok(fields);
    }

    private static async Task<IResult> GetIssuesAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, ct);
        if (user is null) return Results.Unauthorized();

        var exists = await db.ImportRuns.AnyAsync(r => r.Id == id, ct);
        if (!exists) return Results.NotFound();

        var issues = await db.ValidationIssues
            .Where(i => i.ImportRunId == id)
            .OrderBy(i => i.Severity)
            .Select(i => new ValidationIssueResponse(
                i.Id, i.Severity.ToString(), i.FieldKey, i.Message))
            .ToListAsync(ct);

        return Results.Ok(issues);
    }

    private static async Task<IResult> ApproveAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
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
        Guid id,
        ReviewDecisionRequest req,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
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
        Guid id,
        ReviewDecisionRequest req,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
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
        Guid runId,
        Guid fieldId,
        ConfirmFieldRequest req,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
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
            field.Id, field.FieldKey, field.FieldValue, field.Confidence, field.SourceRef, field.IsConfirmed));
    }

    private static ImportRunResponse ToRunResponse(
        ImportRun run, string docName, int fieldCount, int issueCount) =>
        new(run.Id, run.DocumentId, docName, run.Status, run.OperatorNotes,
            run.ErrorMessage, run.ExtractedAtUtc, run.CreatedAtUtc, fieldCount, issueCount);
}
