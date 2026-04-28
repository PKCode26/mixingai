using MixingAI.Api.Core.Auth;
using MixingAI.Api.Core.Documents;
using MixingAI.Api.Core.Import;
using MixingAI.Api.Core.Users;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AuthSession> AuthSessions => Set<AuthSession>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ImportRun> ImportRuns => Set<ImportRun>();
    public DbSet<StagedField> StagedFields => Set<StagedField>();
    public DbSet<ValidationIssue> ValidationIssues => Set<ValidationIssue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
