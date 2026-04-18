using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "requester_name",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "reviewer_name",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "target_worker_name",
                schema: "task_operations",
                table: "task_swap_requests",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requester_name",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "reviewer_name",
                schema: "task_operations",
                table: "task_swap_requests");

            migrationBuilder.DropColumn(
                name: "target_worker_name",
                schema: "task_operations",
                table: "task_swap_requests");
        }
    }
}
