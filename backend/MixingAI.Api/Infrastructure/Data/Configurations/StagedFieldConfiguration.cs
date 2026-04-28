using MixingAI.Api.Core.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MixingAI.Api.Infrastructure.Data.Configurations;

public sealed class StagedFieldConfiguration : IEntityTypeConfiguration<StagedField>
{
    public void Configure(EntityTypeBuilder<StagedField> builder)
    {
        builder.ToTable("staged_fields", "app_core");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FieldKey).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FieldValue).HasMaxLength(10_000);
        builder.Property(x => x.SourceRef).HasMaxLength(500);

        builder.HasIndex(x => new { x.ImportRunId, x.FieldKey })
               .HasDatabaseName("IX_staged_fields_RunId_Key");
    }
}
