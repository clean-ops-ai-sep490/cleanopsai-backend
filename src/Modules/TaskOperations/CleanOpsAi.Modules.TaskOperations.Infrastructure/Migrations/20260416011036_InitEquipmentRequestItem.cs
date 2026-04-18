using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.TaskOperations.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitEquipmentRequestItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "equipment_id",
                schema: "task_operations",
                table: "equipment_requests");

            migrationBuilder.DropColumn(
                name: "quantity",
                schema: "task_operations",
                table: "equipment_requests");

            migrationBuilder.CreateTable(
                name: "equipment_request_items",
                schema: "task_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_equipment_request_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_equipment_request_items_equipment_requests_equipment_reques",
                        column: x => x.equipment_request_id,
                        principalSchema: "task_operations",
                        principalTable: "equipment_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_equipment_request_items_equipment_id",
                schema: "task_operations",
                table: "equipment_request_items",
                column: "equipment_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_request_items_equipment_request_id",
                schema: "task_operations",
                table: "equipment_request_items",
                column: "equipment_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_equipment_request_items_equipment_request_id_equipment_id",
                schema: "task_operations",
                table: "equipment_request_items",
                columns: new[] { "equipment_request_id", "equipment_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipment_request_items",
                schema: "task_operations");

            migrationBuilder.AddColumn<Guid>(
                name: "equipment_id",
                schema: "task_operations",
                table: "equipment_requests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "quantity",
                schema: "task_operations",
                table: "equipment_requests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
