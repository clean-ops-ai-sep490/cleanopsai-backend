using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Scoring.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringRetrainRunLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "logs",
                schema: "scoring",
                table: "scoring_retrain_runs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "logs",
                schema: "scoring",
                table: "scoring_retrain_runs");
        }
    }
}
