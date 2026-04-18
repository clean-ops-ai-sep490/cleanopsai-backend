using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "workarea_checkin");

            migrationBuilder.CreateTable(
                name: "workarea_checkin_points",
                schema: "workarea_checkin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workarea_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workarea_checkin_points", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "access_devices",
                schema: "workarea_checkin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workarea_checkin_point_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    identifier = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    position_x = table.Column<decimal>(type: "numeric", nullable: true),
                    position_y = table.Column<decimal>(type: "numeric", nullable: true),
                    position_z = table.Column<decimal>(type: "numeric", nullable: true),
                    position_floor_level = table.Column<int>(type: "integer", nullable: true),
                    position_installation_location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ble_info_battery_level = table.Column<int>(type: "integer", nullable: true),
                    ble_info_last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ble_info_last_maintenance_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_access_devices", x => x.id);
                    table.ForeignKey(
                        name: "fk_access_devices_workarea_checkin_points_workarea_checkin_poi",
                        column: x => x.workarea_checkin_point_id,
                        principalSchema: "workarea_checkin",
                        principalTable: "workarea_checkin_points",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "checkin_records",
                schema: "workarea_checkin",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    worker_id = table.Column<Guid>(type: "uuid", nullable: false),
                    workarea_checkin_point_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_device_id = table.Column<Guid>(type: "uuid", nullable: true),
                    task_id = table.Column<Guid>(type: "uuid", nullable: true),
                    task_step_id = table.Column<Guid>(type: "uuid", nullable: true),
                    checkin_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checkin_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkin_records", x => x.id);
                    table.ForeignKey(
                        name: "fk_checkin_records_access_devices_access_device_id",
                        column: x => x.access_device_id,
                        principalSchema: "workarea_checkin",
                        principalTable: "access_devices",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_checkin_records_workarea_checkin_points_workarea_checkin_po",
                        column: x => x.workarea_checkin_point_id,
                        principalSchema: "workarea_checkin",
                        principalTable: "workarea_checkin_points",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_access_devices_workarea_checkin_point_id",
                schema: "workarea_checkin",
                table: "access_devices",
                column: "workarea_checkin_point_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkin_records_access_device_id",
                schema: "workarea_checkin",
                table: "checkin_records",
                column: "access_device_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkin_records_checkin_at",
                schema: "workarea_checkin",
                table: "checkin_records",
                column: "checkin_at");

            migrationBuilder.CreateIndex(
                name: "ix_checkin_records_workarea_checkin_point_id_checkin_at",
                schema: "workarea_checkin",
                table: "checkin_records",
                columns: new[] { "workarea_checkin_point_id", "checkin_at" });

            migrationBuilder.CreateIndex(
                name: "ix_checkin_records_worker_id",
                schema: "workarea_checkin",
                table: "checkin_records",
                column: "worker_id");

            migrationBuilder.CreateIndex(
                name: "ix_workarea_checkin_points_code",
                schema: "workarea_checkin",
                table: "workarea_checkin_points",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checkin_records",
                schema: "workarea_checkin");

            migrationBuilder.DropTable(
                name: "access_devices",
                schema: "workarea_checkin");

            migrationBuilder.DropTable(
                name: "workarea_checkin_points",
                schema: "workarea_checkin");
        }
    }
}
