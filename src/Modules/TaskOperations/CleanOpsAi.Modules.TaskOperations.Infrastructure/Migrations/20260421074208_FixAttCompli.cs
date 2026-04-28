using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAttCompli : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "quality_score",
                schema: "task_operations",
                table: "task_step_execution_images",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verdict",
                schema: "task_operations",
                table: "task_step_execution_images",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                schema: "task_operations",
                table: "compliance_checks",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "ai_result_raw",
                schema: "task_operations",
                table: "compliance_checks",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "failed_image_count",
                schema: "task_operations",
                table: "compliance_checks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "min_score",
                schema: "task_operations",
                table: "compliance_checks",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "supervisor_id",
                schema: "task_operations",
                table: "compliance_checks",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quality_score",
                schema: "task_operations",
                table: "task_step_execution_images");

            migrationBuilder.DropColumn(
                name: "verdict",
                schema: "task_operations",
                table: "task_step_execution_images");

            migrationBuilder.DropColumn(
                name: "ai_result_raw",
                schema: "task_operations",
                table: "compliance_checks");

            migrationBuilder.DropColumn(
                name: "failed_image_count",
                schema: "task_operations",
                table: "compliance_checks");

            migrationBuilder.DropColumn(
                name: "min_score",
                schema: "task_operations",
                table: "compliance_checks");

            migrationBuilder.DropColumn(
                name: "supervisor_id",
                schema: "task_operations",
                table: "compliance_checks");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                schema: "task_operations",
                table: "compliance_checks",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
