using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.Workforce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategorySkillCerti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "longitude",
                schema: "workforce",
                table: "worker_gps",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<double>(
                name: "latitude",
                schema: "workforce",
                table: "worker_gps",
                type: "double precision",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "category",
                schema: "workforce",
                table: "skill",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "category",
                schema: "workforce",
                table: "certifications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                schema: "workforce",
                table: "skill");

            migrationBuilder.DropColumn(
                name: "category",
                schema: "workforce",
                table: "certifications");

            migrationBuilder.AlterColumn<double>(
                name: "longitude",
                schema: "workforce",
                table: "worker_gps",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "latitude",
                schema: "workforce",
                table: "worker_gps",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldNullable: true);
        }
    }
}
