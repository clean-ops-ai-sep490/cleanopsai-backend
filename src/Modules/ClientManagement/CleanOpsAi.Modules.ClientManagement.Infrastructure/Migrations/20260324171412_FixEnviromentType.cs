using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ClientManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixEnviromentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "environment_type",
                schema: "client_management",
                table: "sla");

            migrationBuilder.AddColumn<Guid>(
                name: "environment_type_id",
                schema: "client_management",
                table: "sla",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "environment_type_id",
                schema: "client_management",
                table: "sla");

            migrationBuilder.AddColumn<int>(
                name: "environment_type",
                schema: "client_management",
                table: "sla",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
