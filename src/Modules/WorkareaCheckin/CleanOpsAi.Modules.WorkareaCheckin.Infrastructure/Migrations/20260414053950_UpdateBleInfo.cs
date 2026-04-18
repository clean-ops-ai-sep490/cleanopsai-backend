using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBleInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "metadata",
                schema: "workarea_checkin",
                table: "access_devices",
                newName: "ble_info_service_uuid");

            migrationBuilder.AddColumn<int>(
                name: "ble_info_last_rssi",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ble_info_rssi_threshold",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ble_info_tx_power",
                schema: "workarea_checkin",
                table: "access_devices",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ble_info_last_rssi",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.DropColumn(
                name: "ble_info_rssi_threshold",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.DropColumn(
                name: "ble_info_tx_power",
                schema: "workarea_checkin",
                table: "access_devices");

            migrationBuilder.RenameColumn(
                name: "ble_info_service_uuid",
                schema: "workarea_checkin",
                table: "access_devices",
                newName: "metadata");
        }
    }
}
