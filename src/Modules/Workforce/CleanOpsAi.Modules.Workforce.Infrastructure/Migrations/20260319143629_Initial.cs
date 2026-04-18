using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workforce");

            migrationBuilder.CreateTable(
                name: "certifications",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    issuing_organization = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_certifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "equipments",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "skill",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_skill", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workers",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "text", nullable: false),
                    display_address = table.Column<string>(type: "text", nullable: true),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    avatar_url = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "worker_certifications",
                schema: "workforce",
                columns: table => new
                {
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    certification_id = table.Column<Guid>(type: "uuid", nullable: false),
                    issued_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_certifications", x => new { x.worker_id, x.certification_id });
                    table.ForeignKey(
                        name: "fk_worker_certifications_certifications_certification_id",
                        column: x => x.certification_id,
                        principalSchema: "workforce",
                        principalTable: "certifications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_worker_certifications_workers_worker_id",
                        column: x => x.worker_id,
                        principalSchema: "workforce",
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_gps",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_gps", x => x.id);
                    table.ForeignKey(
                        name: "fk_worker_gps_workers_worker_id",
                        column: x => x.worker_id,
                        principalSchema: "workforce",
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "worker_skills",
                schema: "workforce",
                columns: table => new
                {
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_worker_skills", x => new { x.worker_id, x.skill_id });
                    table.ForeignKey(
                        name: "fk_worker_skills_skill_skill_id",
                        column: x => x.skill_id,
                        principalSchema: "workforce",
                        principalTable: "skill",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_worker_skills_workers_worker_id",
                        column: x => x.worker_id,
                        principalSchema: "workforce",
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_worker_certifications_certification_id",
                schema: "workforce",
                table: "worker_certifications",
                column: "certification_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_gps_worker_id",
                schema: "workforce",
                table: "worker_gps",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_worker_skills_skill_id",
                schema: "workforce",
                table: "worker_skills",
                column: "skill_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipments",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "worker_certifications",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "worker_gps",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "worker_skills",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "certifications",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "skill",
                schema: "workforce");

            migrationBuilder.DropTable(
                name: "workers",
                schema: "workforce");
        }
    }
}
