using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttFcmWk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "worker_id",
                schema: "quality_control",
                table: "fcm_tokens",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "worker_id",
                schema: "quality_control",
                table: "fcm_tokens");
        }
    }
}
