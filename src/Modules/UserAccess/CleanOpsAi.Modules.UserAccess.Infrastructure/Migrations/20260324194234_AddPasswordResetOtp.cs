using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.UserAccess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetOtp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetOtps",
                schema: "user_access",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    otp_code = table.Column<string>(type: "text", nullable: false),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_used = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_password_reset_otps", x => x.id);
                });

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "concurrency_stamp",
                value: "11111111-1111-1111-1111-111111111111");

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "concurrency_stamp",
                value: "22222222-2222-2222-2222-222222222222");

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "concurrency_stamp",
                value: "33333333-3333-3333-3333-333333333333");

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "concurrency_stamp",
                value: "44444444-4444-4444-4444-444444444444");

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "concurrency_stamp",
                value: "55555555-5555-5555-5555-555555555555");

            migrationBuilder.CreateIndex(
                name: "ix_password_reset_otps_email",
                schema: "user_access",
                table: "PasswordResetOtps",
                column: "email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordResetOtps",
                schema: "user_access");

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "concurrency_stamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "concurrency_stamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "concurrency_stamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "concurrency_stamp",
                value: null);

            migrationBuilder.UpdateData(
                schema: "user_access",
                table: "AspNetRoles",
                keyColumn: "id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"),
                column: "concurrency_stamp",
                value: null);
        }
    }
}
