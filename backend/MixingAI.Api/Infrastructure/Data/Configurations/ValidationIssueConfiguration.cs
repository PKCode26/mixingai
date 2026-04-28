using MixingAI.Api.Core.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MixingAI.Api.Infrastructure.Data.Configurations;

public sealed class ValidationIssueConfiguration : IEntityTypeConfiguration<ValidationIssue>
{
    public void Configure(EntityTypeBuilder<ValidationIssue> builder)
    {
        builder.ToTable("validation_issues", "app_core");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.FieldKey).HasMaxLength(200);
        builder.Property(x => x.Severity).IsRequired();

        builder.HasIndex(x => x.ImportRunId).HasDatabaseName("IX_validation_issues_RunId");
    }
}
