using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "task_operations");

            migrationBuilder.CreateTable(
                name: "task_assignments",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_assignee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    scheduled_start_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_adhoc_task = table.Column<bool>(type: "boolean", nullable: false),
                    name_adhoc_task = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_assignments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "adhoc_requests",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requested_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    request_type = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_adhoc_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_adhoc_requests_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "emergency_leave_requests",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    audio_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    transcription = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emergency_leave_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_emergency_leave_requests_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "equipment_requests",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewed_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipment_requests_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_reports",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_by_worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    resolved_by_user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    resolved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_issue_reports", x => x.id);
                    table.ForeignKey(
                        name: "fk_issue_reports_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_history",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_history_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_step_executions",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sop_step_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    result_data = table.Column<string>(type: "jsonb", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_step_executions", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_step_executions_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_swap_requests",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_assignment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    requester_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_worker_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    reviewed_by = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_swap_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_swap_requests_task_assignments_task_assignment_id",
                        column: x => x.task_assignment_id,
                        principalSchema: "task_operations",
                        principalTable: "task_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "compliance_checks",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_step_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    feedback = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compliance_checks", x => x.id);
                    table.ForeignKey(
                        name: "fk_compliance_checks_task_step_executions_task_step_execution_",
                        column: x => x.task_step_execution_id,
                        principalSchema: "task_operations",
                        principalTable: "task_step_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_step_execution_images",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_step_execution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    image_type = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_step_execution_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_step_execution_images_task_step_executions_task_step_e",
                        column: x => x.task_step_execution_id,
                        principalSchema: "task_operations",
                        principalTable: "task_step_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_adhoc_requests_task_assignment_id",
                schema: "task_operations",
                table: "adhoc_requests",
                column: "task_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_compliance_checks_task_step_execution_id",
                schema: "task_operations",
                table: "compliance_checks",
                column: "task_step_execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_emergency_leave_requests_task_assignment_id",
                schema: "task_operations",
                table: "emergency_leave_requests",
                column: "task_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_requests_task_assignment_id",
                schema: "task_operations",
                table: "equipment_requests",
                column: "task_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_requests_worker_id",
                schema: "task_operations",
                table: "equipment_requests",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_issue_reports_reported_by_worker_id",
                schema: "task_operations",
                table: "issue_reports",
                column: "reported_by_worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_issue_reports_task_assignment_id",
                schema: "task_operations",
                table: "issue_reports",
                column: "task_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_assignee_id",
                schema: "task_operations",
                table: "task_assignments",
                column: "assignee_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_status",
                schema: "task_operations",
                table: "task_assignments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_task_schedule_id",
                schema: "task_operations",
                table: "task_assignments",
                column: "task_schedule_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_assignments_task_schedule_id_scheduled_start_at",
                schema: "task_operations",
                table: "task_assignments",
                columns: new[] { "task_schedule_id", "scheduled_start_at" },
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_task_history_task_assignment_id",
                schema: "task_operations",
                table: "task_history",
                column: "task_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_step_execution_images_task_step_execution_id",
                schema: "task_operations",
                table: "task_step_execution_images",
                column: "task_step_execution_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_step_executions_task_assignment_id",
                schema: "task_operations",
                table: "task_step_executions",
                column: "task_assignment_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_swap_requests_task_assignment_id",
                schema: "task_operations",
                table: "task_swap_requests",
                column: "task_assignment_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adhoc_requests",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "compliance_checks",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "emergency_leave_requests",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "equipment_requests",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "issue_reports",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "task_history",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "task_step_execution_images",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "task_swap_requests",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "task_step_executions",
                schema: "task_operations");

            migrationBuilder.DropTable(
                name: "task_assignments",
                schema: "task_operations");
        }
    }
}
