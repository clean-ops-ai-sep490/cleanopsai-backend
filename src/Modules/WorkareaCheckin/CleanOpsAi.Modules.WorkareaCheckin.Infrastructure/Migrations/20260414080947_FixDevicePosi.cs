using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDevicePosi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "position_floor_level",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.DropColumn(
                name: "position_x",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.DropColumn(
                name: "position_y",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.DropColumn(
                name: "position_z",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.RenameColumn(
                name: "position_installation_location",
                schema: "workarea_checkin",
                table: "access_devices",
                newName: "installation_location");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "installation_location",
                schema: "workarea_checkin",
                table: "access_devices",
                newName: "position_installation_location");

            migrationBuilder.AddColumn<int>(
                name: "position_floor_level",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "position_x",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "position_y",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "position_z",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "numeric",
                nullable: true);
        }
    }
}
