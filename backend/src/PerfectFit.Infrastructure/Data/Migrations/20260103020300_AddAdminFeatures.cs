using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerfectFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "role",
                table: "users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "admin_audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    admin_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    action = table.Column<int>(type: "INTEGER", nullable: false),
                    target_user_id = table.Column<Guid>(type: "TEXT", nullable: true),
                    details = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_audit_logs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_audit_logs_action",
                table: "admin_audit_logs",
                column: "action");

            migrationBuilder.CreateIndex(
                name: "ix_admin_audit_logs_admin_user_id",
                table: "admin_audit_logs",
                column: "admin_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_admin_audit_logs_timestamp",
                table: "admin_audit_logs",
                column: "timestamp",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_audit_logs");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "role",
                table: "users");
        }
    }
}
