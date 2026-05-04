using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanOpsAi.Modules.QualityControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixRecipientId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notification_recipients_notification_id_recipient_id",
                schema: "quality_control",
                table: "notification_recipients");

            migrationBuilder.AlterColumn<Guid>(
                name: "recipient_id",
                schema: "quality_control",
                table: "notification_recipients",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateIndex(
                name: "ix_notification_recipients_notification_id_recipient_id",
                schema: "quality_control",
                table: "notification_recipients",
                columns: new[] { "notification_id", "recipient_id" },
                unique: true,
                filter: "is_deleted = false AND recipient_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_notification_recipients_notification_id_recipient_id",
                schema: "quality_control",
                table: "notification_recipients");

            migrationBuilder.AlterColumn<Guid>(
                name: "recipient_id",
                schema: "quality_control",
                table: "notification_recipients",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_recipients_notification_id_recipient_id",
                schema: "quality_control",
                table: "notification_recipients",
                columns: new[] { "notification_id", "recipient_id" },
                unique: true,
                filter: "is_deleted = false");
        }
    }
}
