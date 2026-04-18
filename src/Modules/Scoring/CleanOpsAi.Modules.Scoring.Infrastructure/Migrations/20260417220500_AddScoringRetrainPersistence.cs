using System;
using CleanOpsAi.Modules.Scoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Migrations
{
    [DbContext(typeof(ScoringDbContext))]
    [Migration("20260417220500_AddScoringRetrainPersistence")]
    public partial class AddScoringRetrainPersistence : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "scoring_retrain_batches",
                schema: "scoring",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    source_window_from_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    reviewed_sample_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    promoted = table.Column<bool>(type: "boolean", nullable: false),
                    metric_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    candidate_metric = table.Column<double>(type: "double precision", nullable: true),
                    baseline_metric = table.Column<double>(type: "double precision", nullable: true),
                    minimum_improvement = table.Column<double>(type: "double precision", nullable: true),
                    promotion_reason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoring_retrain_batches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scoring_retrain_runs",
                schema: "scoring",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    scoring_retrain_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    exit_code = table.Column<int>(type: "integer", nullable: true),
                    message = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoring_retrain_runs", x => x.id);
                    table.ForeignKey(
                        name: "fk_scoring_retrain_runs_scoring_retrain_batches_scoring_retrai_8d4a2f5c",
                        column: x => x.scoring_retrain_batch_id,
                        principalSchema: "scoring",
                        principalTable: "scoring_retrain_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_scoring_retrain_batches_created",
                schema: "scoring",
                table: "scoring_retrain_batches",
                column: "created");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_retrain_batches_requested_at_utc",
                schema: "scoring",
                table: "scoring_retrain_batches",
                column: "requested_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_retrain_batches_status",
                schema: "scoring",
                table: "scoring_retrain_batches",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_retrain_runs_scoring_retrain_batch_id",
                schema: "scoring",
                table: "scoring_retrain_runs",
                column: "scoring_retrain_batch_id");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_retrain_runs_started_at_utc",
                schema: "scoring",
                table: "scoring_retrain_runs",
                column: "started_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_retrain_runs_status",
                schema: "scoring",
                table: "scoring_retrain_runs",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "scoring_retrain_runs",
                schema: "scoring");

            migrationBuilder.DropTable(
                name: "scoring_retrain_batches",
                schema: "scoring");
        }
    }
}
