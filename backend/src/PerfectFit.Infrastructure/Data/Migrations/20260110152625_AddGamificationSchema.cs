using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PerfectFit.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGamificationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "current_season_tier",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "current_streak",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "current_win_streak",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "equipped_avatar_frame_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_badge_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "equipped_board_theme_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "fast_games",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "high_accuracy_games",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_played_date",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_streak_notification_sent_at",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "longest_streak",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "night_games",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "perfect_games",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "season_pass_xp",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "streak_freeze_tokens",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "timezone",
                table: "users",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_wins",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "achievements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    icon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    unlock_condition = table.Column<string>(type: "jsonb", nullable: false),
                    reward_type = table.Column<int>(type: "integer", nullable: false),
                    reward_value = table.Column<int>(type: "integer", nullable: false),
                    is_secret = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    reward_cosmetic_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_achievements", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "challenge_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_challenge_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "challenges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_challenges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cosmetics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    asset_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    preview_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    rarity = table.Column<int>(type: "integer", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cosmetics", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "personal_goals",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    current_value = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personal_goals", x => x.id);
                    table.ForeignKey(
                        name: "FK_personal_goals_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "seasons",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    number = table.Column<int>(type: "integer", nullable: false),
                    theme = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seasons", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_achievements",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    achievement_id = table.Column<int>(type: "integer", nullable: false),
                    unlocked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_achievements", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_achievements_achievements_achievement_id",
                        column: x => x.achievement_id,
                        principalTable: "achievements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_achievements_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_challenges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    challenge_id = table.Column<int>(type: "integer", nullable: false),
                    current_progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_challenges", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_challenges_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_challenges_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_cosmetics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    cosmetic_id = table.Column<int>(type: "integer", nullable: false),
                    obtained_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    obtained_from = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_cosmetics", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_cosmetics_cosmetics_cosmetic_id",
                        column: x => x.cosmetic_id,
                        principalTable: "cosmetics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_cosmetics_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "season_archives",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    season_id = table.Column<int>(type: "integer", nullable: false),
                    final_xp = table.Column<int>(type: "integer", nullable: false),
                    final_tier = table.Column<int>(type: "integer", nullable: false),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_season_archives", x => x.id);
                    table.ForeignKey(
                        name: "FK_season_archives_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_season_archives_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "season_rewards",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    season_id = table.Column<int>(type: "integer", nullable: false),
                    tier = table.Column<int>(type: "integer", nullable: false),
                    reward_type = table.Column<int>(type: "integer", nullable: false),
                    reward_value = table.Column<int>(type: "integer", nullable: false),
                    xp_required = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_season_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_season_rewards_seasons_season_id",
                        column: x => x.season_id,
                        principalTable: "seasons",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "claimed_season_rewards",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    season_reward_id = table.Column<int>(type: "integer", nullable: false),
                    claimed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_claimed_season_rewards", x => x.id);
                    table.ForeignKey(
                        name: "FK_claimed_season_rewards_season_rewards_season_reward_id",
                        column: x => x.season_reward_id,
                        principalTable: "season_rewards",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_claimed_season_rewards_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_achievements_category",
                table: "achievements",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_achievements_name",
                table: "achievements",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_challenge_templates_is_active",
                table: "challenge_templates",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_templates_type",
                table: "challenge_templates",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_templates_type_is_active",
                table: "challenge_templates",
                columns: new[] { "type", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_challenges_active_dates",
                table: "challenges",
                columns: new[] { "is_active", "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ix_challenges_type",
                table: "challenges",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_claimed_season_rewards_season_reward_id",
                table: "claimed_season_rewards",
                column: "season_reward_id");

            migrationBuilder.CreateIndex(
                name: "ix_claimed_season_rewards_user",
                table: "claimed_season_rewards",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_claimed_season_rewards_user_reward",
                table: "claimed_season_rewards",
                columns: new[] { "user_id", "season_reward_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cosmetics_code",
                table: "cosmetics",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cosmetics_is_default",
                table: "cosmetics",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "ix_cosmetics_name",
                table: "cosmetics",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cosmetics_type",
                table: "cosmetics",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_personal_goals_user",
                table: "personal_goals",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_personal_goals_user_completed",
                table: "personal_goals",
                columns: new[] { "user_id", "is_completed" });

            migrationBuilder.CreateIndex(
                name: "IX_season_archives_season_id",
                table: "season_archives",
                column: "season_id");

            migrationBuilder.CreateIndex(
                name: "ix_season_archives_user_season_unique",
                table: "season_archives",
                columns: new[] { "user_id", "season_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_season_rewards_season_tier",
                table: "season_rewards",
                columns: new[] { "season_id", "tier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_seasons_active",
                table: "seasons",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_seasons_number",
                table: "seasons",
                column: "number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_achievements_achievement_id",
                table: "user_achievements",
                column: "achievement_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_achievements_user_achievement",
                table: "user_achievements",
                columns: new[] { "user_id", "achievement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_achievements_user_unlocked",
                table: "user_achievements",
                columns: new[] { "user_id", "unlocked_at" });

            migrationBuilder.CreateIndex(
                name: "IX_user_challenges_challenge_id",
                table: "user_challenges",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_challenges_user_challenge",
                table: "user_challenges",
                columns: new[] { "user_id", "challenge_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_challenges_user_completed",
                table: "user_challenges",
                columns: new[] { "user_id", "is_completed" });

            migrationBuilder.CreateIndex(
                name: "IX_user_cosmetics_cosmetic_id",
                table: "user_cosmetics",
                column: "cosmetic_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_cosmetics_user",
                table: "user_cosmetics",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_cosmetics_user_cosmetic",
                table: "user_cosmetics",
                columns: new[] { "user_id", "cosmetic_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "challenge_templates");

            migrationBuilder.DropTable(
                name: "claimed_season_rewards");

            migrationBuilder.DropTable(
                name: "personal_goals");

            migrationBuilder.DropTable(
                name: "season_archives");

            migrationBuilder.DropTable(
                name: "user_achievements");

            migrationBuilder.DropTable(
                name: "user_challenges");

            migrationBuilder.DropTable(
                name: "user_cosmetics");

            migrationBuilder.DropTable(
                name: "season_rewards");

            migrationBuilder.DropTable(
                name: "achievements");

            migrationBuilder.DropTable(
                name: "challenges");

            migrationBuilder.DropTable(
                name: "cosmetics");

            migrationBuilder.DropTable(
                name: "seasons");

            migrationBuilder.DropColumn(
                name: "current_season_tier",
                table: "users");

            migrationBuilder.DropColumn(
                name: "current_streak",
                table: "users");

            migrationBuilder.DropColumn(
                name: "current_win_streak",
                table: "users");

            migrationBuilder.DropColumn(
                name: "equipped_avatar_frame_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "equipped_badge_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "equipped_board_theme_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "fast_games",
                table: "users");

            migrationBuilder.DropColumn(
                name: "high_accuracy_games",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_played_date",
                table: "users");

            migrationBuilder.DropColumn(
                name: "last_streak_notification_sent_at",
                table: "users");

            migrationBuilder.DropColumn(
                name: "longest_streak",
                table: "users");

            migrationBuilder.DropColumn(
                name: "night_games",
                table: "users");

            migrationBuilder.DropColumn(
                name: "perfect_games",
                table: "users");

            migrationBuilder.DropColumn(
                name: "season_pass_xp",
                table: "users");

            migrationBuilder.DropColumn(
                name: "streak_freeze_tokens",
                table: "users");

            migrationBuilder.DropColumn(
                name: "timezone",
                table: "users");

            migrationBuilder.DropColumn(
                name: "total_wins",
                table: "users");
        }
    }
}
