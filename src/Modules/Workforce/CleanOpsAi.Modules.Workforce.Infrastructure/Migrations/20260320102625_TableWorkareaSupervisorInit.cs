using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TableWorkareaSupervisorInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workarea_supervisors",
                schema: "workforce",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    work_area_id = table.Column<Guid>(type: "uuid", nullable: true),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workarea_supervisors", x => x.id);
                    table.ForeignKey(
                        name: "fk_workarea_supervisors_workers_worker_id",
                        column: x => x.worker_id,
                        principalSchema: "workforce",
                        principalTable: "workers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workarea_supervisors_user_id",
                schema: "workforce",
                table: "workarea_supervisors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_workarea_supervisors_work_area_id",
                schema: "workforce",
                table: "workarea_supervisors",
                column: "work_area_id");

            migrationBuilder.CreateIndex(
                name: "ix_workarea_supervisors_work_area_id_worker_id_user_id",
                schema: "workforce",
                table: "workarea_supervisors",
                columns: new[] { "work_area_id", "worker_id", "user_id" },
                unique: true,
                filter: "\"work_area_id\" IS NOT NULL AND \"worker_id\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_workarea_supervisors_worker_id",
                schema: "workforce",
                table: "workarea_supervisors",
                column: "worker_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workarea_supervisors",
                schema: "workforce");
        }
    }
}
