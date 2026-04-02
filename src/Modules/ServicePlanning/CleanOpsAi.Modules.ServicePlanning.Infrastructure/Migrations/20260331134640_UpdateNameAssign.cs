using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNameAssign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "assignee_name",
                schema: "service_planning",
                table: "task_schedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "display_location",
                schema: "service_planning",
                table: "task_schedules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "assignee_name",
                schema: "service_planning",
                table: "task_schedules");

            migrationBuilder.DropColumn(
                name: "display_location",
                schema: "service_planning",
                table: "task_schedules");
        }
    }
}
