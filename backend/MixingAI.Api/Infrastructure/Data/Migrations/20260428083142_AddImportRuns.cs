using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixingAI.Api.Infrastructure.Data.Migrations
{
    public partial class AddImportRuns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE TABLE IF NOT EXISTS app_core.import_runs (
    ""Id""              uuid            NOT NULL,
    ""DocumentId""      uuid            NOT NULL,
    ""Status""          integer         NOT NULL DEFAULT 0,
    ""OperatorNotes""   varchar(2000)   NULL,
    ""ErrorMessage""    varchar(2000)   NULL,
    ""ExtractedAtUtc""  timestamptz     NULL,
    ""CreatedAtUtc""    timestamptz     NOT NULL,
    ""CreatedByUserId"" uuid            NULL,
    ""UpdatedAtUtc""    timestamptz     NULL,
    ""UpdatedByUserId"" uuid            NULL,
    CONSTRAINT ""PK_import_runs"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_import_runs_documents"" FOREIGN KEY (""DocumentId"")
        REFERENCES app_core.documents (""Id"") ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ""IX_import_runs_Status""
    ON app_core.import_runs (""Status"");

CREATE INDEX IF NOT EXISTS ""IX_import_runs_DocumentId""
    ON app_core.import_runs (""DocumentId"");

CREATE TABLE IF NOT EXISTS app_core.staged_fields (
    ""Id""           uuid            NOT NULL,
    ""ImportRunId""  uuid            NOT NULL,
    ""FieldKey""     varchar(200)    NOT NULL,
    ""FieldValue""   varchar(10000)  NULL,
    ""Confidence""   real            NULL,
    ""SourceRef""    varchar(500)    NULL,
    ""IsConfirmed""  boolean         NOT NULL DEFAULT false,
    CONSTRAINT ""PK_staged_fields"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_staged_fields_import_runs"" FOREIGN KEY (""ImportRunId"")
        REFERENCES app_core.import_runs (""Id"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_staged_fields_RunId_Key""
    ON app_core.staged_fields (""ImportRunId"", ""FieldKey"");

CREATE TABLE IF NOT EXISTS app_core.validation_issues (
    ""Id""           uuid            NOT NULL,
    ""ImportRunId""  uuid            NOT NULL,
    ""Severity""     integer         NOT NULL DEFAULT 0,
    ""FieldKey""     varchar(200)    NULL,
    ""Message""      varchar(2000)   NOT NULL,
    CONSTRAINT ""PK_validation_issues"" PRIMARY KEY (""Id""),
    CONSTRAINT ""FK_validation_issues_import_runs"" FOREIGN KEY (""ImportRunId"")
        REFERENCES app_core.import_runs (""Id"") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ""IX_validation_issues_RunId""
    ON app_core.validation_issues (""ImportRunId"");
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DROP TABLE IF EXISTS app_core.validation_issues;
DROP TABLE IF EXISTS app_core.staged_fields;
DROP TABLE IF EXISTS app_core.import_runs;
");
        }
    }
}
