using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixingAI.Api.Infrastructure.Data.Migrations
{
    public partial class AddDocuments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS app_core.documents (
                    ""Id"" uuid NOT NULL,
                    ""OriginalFileName"" character varying(500) NOT NULL,
                    ""DisplayName"" character varying(500) NOT NULL,
                    ""MimeContentType"" character varying(200) NOT NULL,
                    ""FileSizeBytes"" bigint NOT NULL,
                    ""ContentHash"" character varying(64) NOT NULL,
                    ""StoragePath"" character varying(1000) NOT NULL,
                    ""DocumentType"" integer NOT NULL,
                    ""IsArchived"" boolean NOT NULL DEFAULT false,
                    ""ArchivedAtUtc"" timestamp with time zone,
                    ""ArchivedByUserId"" uuid,
                    ""CreatedAtUtc"" timestamp with time zone NOT NULL,
                    ""CreatedByUserId"" uuid,
                    ""UpdatedAtUtc"" timestamp with time zone,
                    ""UpdatedByUserId"" uuid,
                    CONSTRAINT ""PK_documents"" PRIMARY KEY (""Id"")
                );

                CREATE INDEX IF NOT EXISTS ""IX_documents_ContentHash"" ON app_core.documents (""ContentHash"");
                CREATE INDEX IF NOT EXISTS ""IX_documents_IsArchived"" ON app_core.documents (""IsArchived"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS app_core.documents;");
        }
    }
}
