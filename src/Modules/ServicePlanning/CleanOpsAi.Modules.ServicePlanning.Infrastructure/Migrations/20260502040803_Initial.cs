using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class Initial : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
				name: "service_planning");

			migrationBuilder.CreateTable(
				name: "environment_types",
				schema: "service_planning",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
					description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_environment_types", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "steps",
				schema: "service_planning",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					action_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
					name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
					description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
					config_schema = table.Column<string>(type: "jsonb", nullable: false),
					is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_steps", x => x.id);
				});

			migrationBuilder.CreateTable(
				name: "sops",
				schema: "service_planning",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
					description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
					service_type = table.Column<int>(type: "integer", nullable: false),
					environment_type_id = table.Column<Guid>(type: "uuid", nullable: false),
					version = table.Column<int>(type: "integer", nullable: false),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_sops", x => x.id);
					table.ForeignKey(
						name: "fk_sops_environment_types_environment_type_id",
						column: x => x.environment_type_id,
						principalSchema: "service_planning",
						principalTable: "environment_types",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "sop_required_certifications",
				schema: "service_planning",
				columns: table => new
				{
					sop_id = table.Column<Guid>(type: "uuid", nullable: false),
					certification_id = table.Column<Guid>(type: "uuid", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_sop_required_certifications", x => new { x.sop_id, x.certification_id });
					table.ForeignKey(
						name: "fk_sop_required_certifications_sops_sop_id",
						column: x => x.sop_id,
						principalSchema: "service_planning",
						principalTable: "sops",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "sop_required_skills",
				schema: "service_planning",
				columns: table => new
				{
					sop_id = table.Column<Guid>(type: "uuid", nullable: false),
					skill_id = table.Column<Guid>(type: "uuid", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_sop_required_skills", x => new { x.sop_id, x.skill_id });
					table.ForeignKey(
						name: "fk_sop_required_skills_sops_sop_id",
						column: x => x.sop_id,
						principalSchema: "service_planning",
						principalTable: "sops",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "sop_steps",
				schema: "service_planning",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					sop_id = table.Column<Guid>(type: "uuid", nullable: false),
					step_id = table.Column<Guid>(type: "uuid", nullable: false),
					step_order = table.Column<int>(type: "integer", nullable: false),
					config_detail = table.Column<string>(type: "jsonb", nullable: false),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_sop_steps", x => x.id);
					table.ForeignKey(
						name: "fk_sop_steps_sops_sop_id",
						column: x => x.sop_id,
						principalSchema: "service_planning",
						principalTable: "sops",
						principalColumn: "id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "fk_sop_steps_steps_step_id",
						column: x => x.step_id,
						principalSchema: "service_planning",
						principalTable: "steps",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "task_schedules",
				schema: "service_planning",
				columns: table => new
				{
					id = table.Column<Guid>(type: "uuid", nullable: false),
					sop_id = table.Column<Guid>(type: "uuid", nullable: false),
					sla_task_id = table.Column<Guid>(type: "uuid", nullable: false),
					sla_shift_id = table.Column<Guid>(type: "uuid", nullable: false),
					work_area_id = table.Column<Guid>(type: "uuid", nullable: false),
					work_area_detail_id = table.Column<Guid>(type: "uuid", nullable: true),
					name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
					description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
					assignee_id = table.Column<Guid>(type: "uuid", nullable: true),
					assignee_name = table.Column<string>(type: "text", nullable: true),
					display_location = table.Column<string>(type: "text", nullable: true),
					version = table.Column<int>(type: "integer", nullable: false),
					metadata = table.Column<string>(type: "jsonb", nullable: false),
					duration_minutes = table.Column<int>(type: "integer", nullable: false),
					recurrence_type = table.Column<int>(type: "integer", nullable: false),
					recurrence_config = table.Column<string>(type: "jsonb", nullable: false),
					contract_start_date = table.Column<DateOnly>(type: "date", nullable: false),
					contract_end_date = table.Column<DateOnly>(type: "date", nullable: true),
					last_generated_to_date = table.Column<DateOnly>(type: "date", nullable: true),
					is_active = table.Column<bool>(type: "boolean", nullable: false),
					is_deleted = table.Column<bool>(type: "boolean", nullable: false),
					created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					created_by = table.Column<string>(type: "text", nullable: true),
					last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					last_modified_by = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("pk_task_schedules", x => x.id);
					table.ForeignKey(
						name: "fk_task_schedules_sops_sop_id",
						column: x => x.sop_id,
						principalSchema: "service_planning",
						principalTable: "sops",
						principalColumn: "id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "ix_sop_steps_sop_id",
				schema: "service_planning",
				table: "sop_steps",
				column: "sop_id");

			migrationBuilder.CreateIndex(
				name: "ix_sop_steps_step_id",
				schema: "service_planning",
				table: "sop_steps",
				column: "step_id");

			migrationBuilder.CreateIndex(
				name: "ix_sops_environment_type_id",
				schema: "service_planning",
				table: "sops",
				column: "environment_type_id");

			migrationBuilder.CreateIndex(
				name: "ix_steps_action_key",
				schema: "service_planning",
				table: "steps",
				column: "action_key",
				unique: true,
				filter: "is_deleted = false");

			migrationBuilder.CreateIndex(
				name: "ix_task_schedules_sop_id",
				schema: "service_planning",
				table: "task_schedules",
				column: "sop_id");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "sop_required_certifications",
				schema: "service_planning");

			migrationBuilder.DropTable(
				name: "sop_required_skills",
				schema: "service_planning");

			migrationBuilder.DropTable(
				name: "sop_steps",
				schema: "service_planning");

			migrationBuilder.DropTable(
				name: "task_schedules",
				schema: "service_planning");

			migrationBuilder.DropTable(
				name: "steps",
				schema: "service_planning");

			migrationBuilder.DropTable(
				name: "sops",
				schema: "service_planning");

			migrationBuilder.DropTable(
				name: "environment_types",
				schema: "service_planning");
		}
	}
}