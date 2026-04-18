using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.WorkareaCheckin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUniqueCheckinPoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workarea_checkin_points_code",
                schema: "workarea_checkin",
                table: "workarea_checkin_points");

            migrationBuilder.CreateIndex(
                name: "ix_workarea_checkin_points_workarea_id_code_is_deleted",
                schema: "workarea_checkin",
                table: "workarea_checkin_points",
                columns: new[] { "workarea_id", "code", "is_deleted" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workarea_checkin_points_workarea_id_code_is_deleted",
                schema: "workarea_checkin",
                table: "workarea_checkin_points");

            migrationBuilder.CreateIndex(
                name: "ix_workarea_checkin_points_code",
                schema: "workarea_checkin",
                table: "workarea_checkin_points",
                column: "code",
                unique: true);
        }
    }
}
