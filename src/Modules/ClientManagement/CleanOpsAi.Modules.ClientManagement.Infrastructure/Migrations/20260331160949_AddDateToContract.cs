using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "contract_end_date",
                schema: "client_management",
                table: "contracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "contract_start_date",
                schema: "client_management",
                table: "contracts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "contract_end_date",
                schema: "client_management",
                table: "contracts");

            migrationBuilder.DropColumn(
                name: "contract_start_date",
                schema: "client_management",
                table: "contracts");
        }
    }
}
