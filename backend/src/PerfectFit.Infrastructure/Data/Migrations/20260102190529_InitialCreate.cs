using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PerfectFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_id = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    provider = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    high_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    games_played = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "game_sessions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: true),
                    board_state = table.Column<string>(type: "jsonb", nullable: false),
                    current_pieces = table.Column<string>(type: "jsonb", nullable: false),
                    piece_bag_state = table.Column<string>(type: "jsonb", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    combo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    lines_cleared = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_combo = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_activity_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_sessions", x => x.id);
                    table.ForeignKey(
                        name: "FK_game_sessions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "leaderboard_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<int>(type: "integer", nullable: false),
                    lines_cleared = table.Column<int>(type: "integer", nullable: false),
                    max_combo = table.Column<int>(type: "integer", nullable: false),
                    achieved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    game_session_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leaderboard_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_leaderboard_entries_game_sessions_game_session_id",
                        column: x => x.game_session_id,
                        principalTable: "game_sessions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_leaderboard_entries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_status",
                table: "game_sessions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_user_id",
                table: "game_sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_sessions_user_id_status",
                table: "game_sessions",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_leaderboard_entries_game_session_id",
                table: "leaderboard_entries",
                column: "game_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_leaderboard_entries_score",
                table: "leaderboard_entries",
                column: "score",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_leaderboard_entries_user_id",
                table: "leaderboard_entries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_leaderboard_entries_user_id_score",
                table: "leaderboard_entries",
                columns: new[] { "user_id", "score" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_users_external_id_provider",
                table: "users",
                columns: new[] { "external_id", "provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_high_score",
                table: "users",
                column: "high_score",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "leaderboard_entries");

            migrationBuilder.DropTable(
                name: "game_sessions");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
