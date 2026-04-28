using MixingAI.Api.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MixingAI.Api.Infrastructure.Data.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents", "app_core");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MimeContentType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ContentHash).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StoragePath).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.DocumentType).IsRequired();

        builder.HasIndex(x => x.ContentHash).HasDatabaseName("IX_documents_ContentHash");
        builder.HasIndex(x => x.IsArchived).HasDatabaseName("IX_documents_IsArchived");
    }
}
