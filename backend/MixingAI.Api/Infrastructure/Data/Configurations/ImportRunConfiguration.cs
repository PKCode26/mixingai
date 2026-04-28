using MixingAI.Api.Core.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MixingAI.Api.Infrastructure.Data.Configurations;

public sealed class ImportRunConfiguration : IEntityTypeConfiguration<ImportRun>
{
    public void Configure(EntityTypeBuilder<ImportRun> builder)
    {
        builder.ToTable("import_runs", "app_core");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.OperatorNotes).HasMaxLength(2000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne(x => x.Document)
               .WithMany()
               .HasForeignKey(x => x.DocumentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.StagedFields)
               .WithOne(x => x.ImportRun)
               .HasForeignKey(x => x.ImportRunId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ValidationIssues)
               .WithOne(x => x.ImportRun)
               .HasForeignKey(x => x.ImportRunId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Status).HasDatabaseName("IX_import_runs_Status");
        builder.HasIndex(x => x.DocumentId).HasDatabaseName("IX_import_runs_DocumentId");
    }
}
