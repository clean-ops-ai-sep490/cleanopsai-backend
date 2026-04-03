using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name_adhoc_task",
                schema: "task_operations",
                table: "task_assignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "assignee_name",
                schema: "task_operations",
                table: "task_assignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "original_assignee_name",
                schema: "task_operations",
                table: "task_assignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assignee_name",
                schema: "task_operations",
                table: "task_assignments");

            migrationBuilder.DropColumn(
                name: "original_assignee_name",
                schema: "task_operations",
                table: "task_assignments");

            migrationBuilder.AlterColumn<string>(
                name: "name_adhoc_task",
                schema: "task_operations",
                table: "task_assignments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
