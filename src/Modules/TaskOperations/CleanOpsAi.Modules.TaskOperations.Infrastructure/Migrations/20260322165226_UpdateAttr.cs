using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "target_worker_id",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "expired_at",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "requester_note",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "review_note",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "target_task_assignment_id",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_end_at",
                schema: "task_operations",
                table: "task_assignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "work_area_id",
                schema: "task_operations",
                table: "task_assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_task_swap_requests_target_task_assignment_id",
                schema: "task_operations",
                table: "task_swap_requests",
                column: "target_task_assignment_id");

            migrationBuilder.AddForeignKey(
                name: "fk_task_swap_requests_task_assignments_target_task_assignment_",
                schema: "task_operations",
                table: "task_swap_requests",
                column: "target_task_assignment_id",
                principalSchema: "task_operations",
                principalTable: "task_assignments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_task_swap_requests_task_assignments_target_task_assignment_",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropIndex(
                name: "ix_task_swap_requests_target_task_assignment_id",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "expired_at",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "requester_note",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "review_note",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "target_task_assignment_id",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "scheduled_end_at",
                schema: "task_operations",
                table: "task_assignments");

            migrationBuilder.DropColumn(
                name: "work_area_id",
                schema: "task_operations",
                table: "task_assignments");

            migrationBuilder.AlterColumn<Guid>(
                name: "target_worker_id",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }
    }
}
