using MixingAI.Api.Core.Endpoints;
using MixingAI.Api.Core.Import;
using MixingAI.Api.Core.Import.Extraction;
using MixingAI.Api.Core.Security;
using MixingAI.Api.Core.Services;
using MixingAI.Api.Infrastructure.Data;
using MixingAI.Api.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CurrentUserService>();
builder.Services.AddSingleton<PasswordHashingService>();
builder.Services.AddSingleton<SessionTokenService>();

builder.Services.AddCors(options =>
    options.AddPolicy("LocalDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddSingleton<StorageService>();

builder.Services.AddSingleton<IDocumentExtractor, PdfExtractor>();
builder.Services.AddSingleton<IDocumentExtractor, ExcelExtractor>();
builder.Services.AddHostedService<ImportProcessor>();

builder.Services.AddAntiforgery();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("LocalDev");

    if (app.Configuration.GetValue<bool>("Features:EnableAdminSeedEndpoint"))
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwords = scope.ServiceProvider.GetRequiredService<PasswordHashingService>();
        await AdminSeedService.SeedAsync(db, passwords);
    }
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }))
   .WithName("Health")
   .AllowAnonymous();

app.MapAuthEndpoints();
app.MapDocumentEndpoints();
app.MapImportEndpoints();

app.Run();
