using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scoring");

            migrationBuilder.CreateTable(
                name: "scoring_jobs",
                schema: "scoring",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    environment_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    retry_count = table.Column<int>(type: "integer", nullable: false),
                    submitted_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoring_jobs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scoring_job_results",
                schema: "scoring",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scoring_job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    verdict = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    quality_score = table.Column<double>(type: "double precision", nullable: false),
                    payload_json = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoring_job_results", x => x.id);
                    table.ForeignKey(
                        name: "fk_scoring_job_results_scoring_jobs_scoring_job_id",
                        column: x => x.scoring_job_id,
                        principalSchema: "scoring",
                        principalTable: "scoring_jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scoring_job_results_created",
                schema: "scoring",
                table: "scoring_job_results",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_job_results_scoring_job_id",
                schema: "scoring",
                table: "scoring_job_results",
                column: "scoring_job_id");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_jobs_created",
                schema: "scoring",
                table: "scoring_jobs",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_jobs_request_id",
                schema: "scoring",
                table: "scoring_jobs",
                column: "request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_scoring_jobs_status",
                schema: "scoring",
                table: "scoring_jobs",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scoring_job_results",
                schema: "scoring");

            migrationBuilder.DropTable(
                name: "scoring_jobs",
                schema: "scoring");
        }
    }
}
