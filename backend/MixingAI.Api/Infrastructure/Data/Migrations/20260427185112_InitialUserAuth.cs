using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixingAI.Api.Infrastructure.Data.Migrations
{
    public partial class InitialUserAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Raw SQL with IF NOT EXISTS — workaround for Npgsql 10 EnsureSchema transaction issue
            migrationBuilder.Sql(@"
                CREATE SCHEMA IF NOT EXISTS app_core;

                CREATE TABLE IF NOT EXISTS app_core.auth_sessions (
                    ""Id"" uuid NOT NULL,
                    ""UserId"" uuid NOT NULL,
                    ""TokenHash"" character varying(128) NOT NULL,
                    ""ExpiresAtUtc"" timestamp with time zone NOT NULL,
                    ""RevokedAtUtc"" timestamp with time zone,
                    ""CreatedAtUtc"" timestamp with time zone NOT NULL,
                    CONSTRAINT ""PK_auth_sessions"" PRIMARY KEY (""Id"")
                );

                CREATE TABLE IF NOT EXISTS app_core.users (
                    ""Id"" uuid NOT NULL,
                    ""Username"" character varying(100) NOT NULL,
                    ""Email"" character varying(256) NOT NULL,
                    ""PasswordHash"" character varying(512) NOT NULL,
                    ""FirstName"" character varying(100) NOT NULL,
                    ""LastName"" character varying(100) NOT NULL,
                    ""IsAdmin"" boolean NOT NULL,
                    ""IsActive"" boolean NOT NULL,
                    ""LockedUntilUtc"" timestamp with time zone,
                    ""LastLoginAtUtc"" timestamp with time zone,
                    ""CreatedAtUtc"" timestamp with time zone NOT NULL,
                    ""CreatedByUserId"" uuid,
                    ""UpdatedAtUtc"" timestamp with time zone,
                    ""UpdatedByUserId"" uuid,
                    CONSTRAINT ""PK_users"" PRIMARY KEY (""Id"")
                );

                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_auth_sessions_TokenHash"" ON app_core.auth_sessions (""TokenHash"");
                CREATE INDEX IF NOT EXISTS ""IX_auth_sessions_UserId"" ON app_core.auth_sessions (""UserId"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_users_Email"" ON app_core.users (""Email"");
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_users_Username"" ON app_core.users (""Username"");
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TABLE IF EXISTS app_core.auth_sessions;
                DROP TABLE IF EXISTS app_core.users;
                DROP SCHEMA IF EXISTS app_core;
            ");
        }
    }
}
