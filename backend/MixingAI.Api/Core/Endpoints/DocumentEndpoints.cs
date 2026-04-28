using MixingAI.Api.Core.Contracts;
using MixingAI.Api.Core.Documents;
using MixingAI.Api.Core.Security;
using MixingAI.Api.Core.Services;
using MixingAI.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Core.Endpoints;

public static class DocumentEndpoints
{
    public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/documents");
        group.MapPost("", UploadAsync).DisableAntiforgery();
        group.MapGet("", ListAsync);
        group.MapGet("{id:guid}", GetAsync);
        group.MapGet("{id:guid}/download", DownloadAsync);
        group.MapPost("{id:guid}/archive", ArchiveAsync);
        group.MapPost("{id:guid}/unarchive", UnarchiveAsync);
        return app;
    }

    private static async Task<IResult> UploadAsync(
        HttpContext ctx,
        IFormFile file,
        AppDbContext db,
        StorageService storage,
        CurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, cancellationToken);
        if (user is null) return Results.Unauthorized();

        if (file.Length == 0)
            return Results.BadRequest("Datei ist leer.");

        string hash;
        await using (var stream = file.OpenReadStream())
            hash = StorageService.ComputeHash(stream);

        var existing = await db.Documents
            .FirstOrDefaultAsync(d => d.ContentHash == hash && !d.IsArchived, cancellationToken);
        if (existing is not null)
            return Results.Conflict(new DocumentDuplicateResponse(existing.Id, existing.OriginalFileName));

        var relativePath = await storage.SaveAsync(file, cancellationToken);
        var now = DateTime.UtcNow;
        var docType = DetectType(file.ContentType, file.FileName);

        var doc = new Document { OriginalFileName = file.FileName };
        doc.Initialize(file.FileName, file.ContentType, file.Length, hash, relativePath, docType);
        doc.SetCreated(user.UserId, now);

        db.Documents.Add(doc);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/documents/{doc.Id}", ToResponse(doc));
    }

    private static async Task<IResult> ListAsync(
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        string? search = null,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, cancellationToken);
        if (user is null) return Results.Unauthorized();

        var query = db.Documents.AsQueryable();
        if (!includeArchived)
            query = query.Where(d => !d.IsArchived);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => EF.Functions.ILike(d.DisplayName, $"%{search}%"));

        var docs = await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => ToResponse(d))
            .ToListAsync(cancellationToken);

        return Results.Ok(docs);
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, cancellationToken);
        if (user is null) return Results.Unauthorized();

        var doc = await db.Documents.FindAsync([id], cancellationToken);
        return doc is null ? Results.NotFound() : Results.Ok(ToResponse(doc));
    }

    private static async Task<IResult> DownloadAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        StorageService storage,
        CurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, cancellationToken);
        if (user is null) return Results.Unauthorized();

        var doc = await db.Documents.FindAsync([id], cancellationToken);
        if (doc is null) return Results.NotFound();

        var stream = storage.OpenRead(doc.StoragePath);
        return Results.File(stream, doc.MimeContentType, doc.OriginalFileName);
    }

    private static async Task<IResult> ArchiveAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, cancellationToken);
        if (user is null) return Results.Unauthorized();

        var doc = await db.Documents.FindAsync([id], cancellationToken);
        if (doc is null) return Results.NotFound();
        if (doc.IsArchived) return Results.Ok(ToResponse(doc));

        doc.Archive(user.UserId, DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(doc));
    }

    private static async Task<IResult> UnarchiveAsync(
        Guid id,
        HttpContext ctx,
        AppDbContext db,
        CurrentUserService currentUser,
        CancellationToken cancellationToken)
    {
        var user = await currentUser.GetCurrentUserAsync(ctx, db, cancellationToken);
        if (user is null) return Results.Unauthorized();

        var doc = await db.Documents.FindAsync([id], cancellationToken);
        if (doc is null) return Results.NotFound();

        doc.Unarchive();
        await db.SaveChangesAsync(cancellationToken);
        return Results.Ok(ToResponse(doc));
    }

    private static DocumentType DetectType(string contentType, string fileName)
    {
        if (contentType is "application/pdf" || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return DocumentType.Pdf;
        if (contentType is "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                        or "application/vnd.ms-excel"
            || fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
            || fileName.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
            return DocumentType.Excel;
        return DocumentType.Other;
    }

    private static DocumentResponse ToResponse(Document d) => new(
        d.Id, d.OriginalFileName, d.DisplayName, d.MimeContentType, d.FileSizeBytes,
        d.ContentHash, d.DocumentType, d.IsArchived, d.ArchivedAtUtc,
        d.CreatedAtUtc, d.CreatedByUserId);
}
