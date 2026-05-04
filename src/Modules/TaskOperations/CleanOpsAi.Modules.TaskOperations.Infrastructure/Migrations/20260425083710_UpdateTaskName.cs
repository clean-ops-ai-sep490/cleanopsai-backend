using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "task_name",
                schema: "task_operations",
                table: "task_assignments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "task_name",
                schema: "task_operations",
                table: "task_assignments");
        }
    }
}
