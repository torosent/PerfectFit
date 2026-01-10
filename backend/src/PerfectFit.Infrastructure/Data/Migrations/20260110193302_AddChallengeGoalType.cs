using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PerfectFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChallengeGoalType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "goal_type",
                table: "challenges",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "goal_type",
                table: "challenge_templates",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "goal_type",
                table: "challenges");

            migrationBuilder.DropColumn(
                name: "goal_type",
                table: "challenge_templates");
        }
    }
}
