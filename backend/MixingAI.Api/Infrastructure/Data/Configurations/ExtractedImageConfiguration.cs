using MixingAI.Api.Core.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MixingAI.Api.Infrastructure.Data.Configurations;

public sealed class ExtractedImageConfiguration : IEntityTypeConfiguration<ExtractedImage>
{
    public void Configure(EntityTypeBuilder<ExtractedImage> builder)
    {
        builder.ToTable("extracted_images", "app_core");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.MimeType).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.ImportRun)
               .WithMany()
               .HasForeignKey(x => x.ImportRunId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ImportRunId).HasDatabaseName("IX_extracted_images_RunId");
    }
}
