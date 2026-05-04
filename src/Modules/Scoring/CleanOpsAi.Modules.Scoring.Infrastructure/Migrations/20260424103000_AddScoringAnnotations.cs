using System;
using CleanOpsAi.Modules.Scoring.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Migrations
{
	[DbContext(typeof(ScoringDbContext))]
	[Migration("20260424103000_AddScoringAnnotations")]
	public partial class AddScoringAnnotations : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "annotated_sample_count",
				schema: "scoring",
				table: "scoring_retrain_batches",
				type: "integer",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "approved_annotation_count",
				schema: "scoring",
				table: "scoring_retrain_batches",
				type: "integer",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AddColumn<int>(
				name: "calibration_sample_count",
				schema: "scoring",
				table: "scoring_retrain_batches",
				type: "integer",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateTable(
				name: "scoring_annotation_candidates",
				schema: "scoring",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					result_id = table.Column<Guid>(type: "uuid", nullable: false),
					job_id = table.Column<Guid>(type: "uuid", nullable: false),
					request_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					environment_key = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
					image_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
					visualization_blob_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
					original_verdict = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					reviewed_verdict = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
					source_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					candidate_status = table.Column<int>(type: "integer", nullable: false),
					assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
					created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					submitted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					approved_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
					snapshot_blob_key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
					metadata_blob_key = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_scoring_annotation_candidates", x => x.id);
					table.ForeignKey(
						name: "fk_scoring_annotation_candidates_scoring_job_results_result_id",
						column: x => x.result_id,
						principalSchema: "scoring",
						principalTable: "scoring_job_results",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "scoring_annotations",
				schema: "scoring",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					candidate_id = table.Column<Guid>(type: "uuid", nullable: false),
					annotation_format = table.Column<int>(type: "integer", nullable: false),
					labels_json = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
					reviewer_note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
					version = table.Column<int>(type: "integer", nullable: false),
					created_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
					approved_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_scoring_annotations", x => x.id);
					table.ForeignKey(
						name: "fk_scoring_annotations_scoring_annotation_candidates_candidate_id",
						column: x => x.candidate_id,
						principalSchema: "scoring",
						principalTable: "scoring_annotation_candidates",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotation_candidates_assigned_to_user_id",
				schema: "scoring",
				table: "scoring_annotation_candidates",
				column: "assigned_to_user_id");

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotation_candidates_candidate_status",
				schema: "scoring",
				table: "scoring_annotation_candidates",
				column: "candidate_status");

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotation_candidates_created_at_utc",
				schema: "scoring",
				table: "scoring_annotation_candidates",
				column: "created_at_utc");

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotation_candidates_environment_key",
				schema: "scoring",
				table: "scoring_annotation_candidates",
				column: "environment_key");

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotation_candidates_result_id",
				schema: "scoring",
				table: "scoring_annotation_candidates",
				column: "result_id",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotations_candidate_id",
				schema: "scoring",
				table: "scoring_annotations",
				column: "candidate_id",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "ix_scoring_annotations_created",
				schema: "scoring",
				table: "scoring_annotations",
				column: "created");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "scoring_annotations",
				schema: "scoring");

			migrationBuilder.DropTable(
				name: "scoring_annotation_candidates",
				schema: "scoring");

			migrationBuilder.DropColumn(
				name: "annotated_sample_count",
				schema: "scoring",
				table: "scoring_retrain_batches");

			migrationBuilder.DropColumn(
				name: "approved_annotation_count",
				schema: "scoring",
				table: "scoring_retrain_batches");

			migrationBuilder.DropColumn(
				name: "calibration_sample_count",
				schema: "scoring",
				table: "scoring_retrain_batches");
		}
	}
}
