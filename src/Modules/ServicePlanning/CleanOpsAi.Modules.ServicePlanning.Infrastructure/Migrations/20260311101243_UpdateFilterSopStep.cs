using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFilterSopStep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_steps_action_key",
                table: "steps");

            migrationBuilder.DropIndex(
                name: "ix_sop_steps_sop_id_step_order",
                table: "sop_steps");

            migrationBuilder.CreateIndex(
                name: "ix_steps_action_key",
                table: "steps",
                column: "action_key",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_sop_steps_sop_id_step_order",
                table: "sop_steps",
                columns: new[] { "sop_id", "step_order" },
                unique: true,
                filter: "is_deleted = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_steps_action_key",
                table: "steps");

            migrationBuilder.DropIndex(
                name: "ix_sop_steps_sop_id_step_order",
                table: "sop_steps");

            migrationBuilder.CreateIndex(
                name: "ix_steps_action_key",
                table: "steps",
                column: "action_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sop_steps_sop_id_step_order",
                table: "sop_steps",
                columns: new[] { "sop_id", "step_order" },
                unique: true);
        }
    }
}
