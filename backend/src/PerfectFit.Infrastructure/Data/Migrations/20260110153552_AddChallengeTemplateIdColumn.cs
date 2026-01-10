using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerfectFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeTemplateIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "challenge_template_id",
                table: "challenges",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_challenges_template_start_date",
                table: "challenges",
                columns: new[] { "challenge_template_id", "start_date" },
                unique: true,
                filter: "challenge_template_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_challenges_template_start_date",
                table: "challenges");

            migrationBuilder.DropColumn(
                name: "challenge_template_id",
                table: "challenges");
        }
    }
}
