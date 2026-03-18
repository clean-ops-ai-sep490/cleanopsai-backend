using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiredSkCer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_required_certification",
                table: "sops");

            migrationBuilder.DropColumn(
                name: "is_required_skill",
                table: "sops");

            migrationBuilder.CreateTable(
                name: "sop_required_certifications",
                columns: table => new
                {
                    sop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    certification_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sop_required_certifications", x => new { x.sop_id, x.certification_id });
                    table.ForeignKey(
                        name: "fk_sop_required_certifications_sops_sop_id",
                        column: x => x.sop_id,
                        principalTable: "sops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sop_required_skills",
                columns: table => new
                {
                    sop_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sop_required_skills", x => new { x.sop_id, x.skill_id });
                    table.ForeignKey(
                        name: "fk_sop_required_skills_sops_sop_id",
                        column: x => x.sop_id,
                        principalTable: "sops",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sop_required_certifications");

            migrationBuilder.DropTable(
                name: "sop_required_skills");

            migrationBuilder.AddColumn<bool>(
                name: "is_required_certification",
                table: "sops",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_required_skill",
                table: "sops",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
