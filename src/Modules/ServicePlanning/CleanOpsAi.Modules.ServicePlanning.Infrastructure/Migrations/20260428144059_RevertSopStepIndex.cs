using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.ServicePlanning.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RevertSopStepIndex : Migration
    {
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
        ALTER TABLE service_planning.sop_steps 
        DROP CONSTRAINT IF EXISTS ix_sop_steps_sop_id_step_order;

        DROP INDEX IF EXISTS service_planning.ix_sop_steps_sop_id_step_order;

        CREATE UNIQUE INDEX ix_sop_steps_sop_id_step_order
        ON service_planning.sop_steps (sop_id, step_order)
        WHERE is_deleted = false;
    ");
		}

		protected override void Down(MigrationBuilder migrationBuilder) { }
	}
}
