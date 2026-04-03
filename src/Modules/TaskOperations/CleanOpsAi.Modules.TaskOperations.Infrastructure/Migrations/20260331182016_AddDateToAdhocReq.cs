using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateToAdhocReq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "request_date_from",
                schema: "task_operations",
                table: "adhoc_requests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "request_date_to",
                schema: "task_operations",
                table: "adhoc_requests",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "request_date_from",
                schema: "task_operations",
                table: "adhoc_requests");

            migrationBuilder.DropColumn(
                name: "request_date_to",
                schema: "task_operations",
                table: "adhoc_requests");
        }
    }
}
