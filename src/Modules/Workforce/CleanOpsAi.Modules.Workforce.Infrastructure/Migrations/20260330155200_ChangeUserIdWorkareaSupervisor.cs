using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserIdWorkareaSupervisor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE workforce.workarea_supervisors 
                ALTER COLUMN user_id TYPE uuid 
                USING user_id::uuid;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE workforce.workarea_supervisors 
                ALTER COLUMN user_id TYPE text;
            ");
        }
    }
}
