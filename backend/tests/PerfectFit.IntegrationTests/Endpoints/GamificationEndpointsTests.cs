using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data;
using PerfectFit.Web.DTOs;
using System.Net;
using System.Net.Http.Json;

namespace PerfectFit.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for gamification API endpoints.
/// Each test creates its own factory instance to ensure database isolation.
/// </summary>
public class GamificationEndpointsTests
{
    #region GET /api/gamification Tests

    [Fact]
    public async Task GetGamificationStatus_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/gamification");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetGamificationStatus_Authenticated_ReturnsStatus()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-gam-status", "gamstatus@test.com", "Gam Status User", AuthProvider.Microsoft);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<GamificationStatusDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Streak.Should().NotBeNull();
        result.ActiveChallenges.Should().NotBeNull();
        result.RecentAchievements.Should().NotBeNull();
        result.EquippedCosmetics.Should().NotBeNull();
        result.ActiveGoals.Should().NotBeNull();
    }

    #endregion

    #region GET /api/gamification/challenges Tests

    [Fact]
    public async Task GetChallenges_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/gamification/challenges");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChallenges_ReturnsActiveChallenges()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-challenges", "challenges@test.com", "Challenges User", AuthProvider.Microsoft);
        db.Users.Add(user);

        // Create an active challenge
        var challenge = Challenge.Create(
            name: "Daily Score",
            description: "Score 500 points",
            type: ChallengeType.Daily,
            targetValue: 500,
            xpReward: 50,
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow.AddDays(1)
        );
        db.Challenges.Add(challenge);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/challenges");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ChallengeDto>>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Count.Should().BeGreaterThan(0);
        result.Should().Contain(c => c.Name == "Daily Score");
    }

    [Fact]
    public async Task GetChallenges_WithTypeFilter_ReturnsFiltered()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-chal-filter", "chalfilter@test.com", "Chal Filter User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var dailyChallenge = Challenge.Create(
            name: "Daily Challenge",
            description: "Daily task",
            type: ChallengeType.Daily,
            targetValue: 100,
            xpReward: 25,
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow.AddDays(1)
        );
        var weeklyChallenge = Challenge.Create(
            name: "Weekly Challenge",
            description: "Weekly task",
            type: ChallengeType.Weekly,
            targetValue: 1000,
            xpReward: 100,
            startDate: DateTime.UtcNow.AddDays(-1),
            endDate: DateTime.UtcNow.AddDays(7)
        );
        db.Challenges.AddRange(dailyChallenge, weeklyChallenge);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/challenges?type=Daily");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<ChallengeDto>>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Should().OnlyContain(c => c.Type == "Daily");
    }

    #endregion

    #region GET /api/gamification/achievements Tests

    [Fact]
    public async Task GetAchievements_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/gamification/achievements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAchievements_ReturnsAllWithProgress()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-achieve", "achieve@test.com", "Achieve User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var achievement = Achievement.Create(
            name: "First Win",
            description: "Complete your first game",
            category: AchievementCategory.Games,
            iconUrl: "/icons/first-win.png",
            unlockCondition: "games_completed >= 1",
            rewardType: RewardType.XPBoost,
            rewardValue: 100
        );
        db.Achievements.Add(achievement);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/achievements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AchievementsDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Achievements.Should().NotBeEmpty();
        result.TotalAchievements.Should().BeGreaterThan(0);
    }

    #endregion

    #region GET /api/gamification/season-pass Tests

    [Fact]
    public async Task GetSeasonPass_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/gamification/season-pass");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSeasonPass_ReturnsCurrentSeason()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-season", "season@test.com", "Season User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var season = Season.Create(
            name: "Test Season",
            number: 1,
            startDate: DateTime.UtcNow.AddDays(-10),
            endDate: DateTime.UtcNow.AddDays(80),
            theme: "Test"
        );
        db.Seasons.Add(season);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/season-pass");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SeasonPassDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        // SeasonPass might be null if no active season, but we added one
        result!.HasActiveSeason.Should().BeTrue();
        result.SeasonPass.Should().NotBeNull();
        result.SeasonPass!.SeasonName.Should().Be("Test Season");
    }

    #endregion

    #region GET /api/gamification/cosmetics Tests

    [Fact]
    public async Task GetCosmetics_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/gamification/cosmetics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCosmetics_ReturnsAllWithOwnership()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-cosmetics", "cosmetics@test.com", "Cosmetics User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var cosmetic = Cosmetic.Create(
            code: "theme_default",
            name: "Default Theme",
            description: "The default board theme",
            type: CosmeticType.BoardTheme,
            assetUrl: "/themes/default.png",
            previewUrl: "/themes/default-preview.png",
            rarity: CosmeticRarity.Common,
            isDefault: true
        );
        db.Cosmetics.Add(cosmetic);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/cosmetics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CosmeticsDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Cosmetics.Should().NotBeEmpty();
        result.Cosmetics.Should().Contain(c => c.Name == "Default Theme");
    }

    [Fact]
    public async Task GetCosmetics_WithTypeFilter_ReturnsFiltered()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-cos-filter", "cosfilter@test.com", "Cos Filter User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var boardTheme = Cosmetic.Create(
            code: "theme_board",
            name: "Board Theme",
            description: "A board theme",
            type: CosmeticType.BoardTheme,
            assetUrl: "/themes/board.png",
            previewUrl: "/themes/board-preview.png",
            rarity: CosmeticRarity.Common,
            isDefault: true
        );
        var badge = Cosmetic.Create(
            code: "badge_test",
            name: "Test Badge",
            description: "A badge",
            type: CosmeticType.Badge,
            assetUrl: "/badges/test.png",
            previewUrl: "/badges/test-preview.png",
            rarity: CosmeticRarity.Rare,
            isDefault: false
        );
        db.Cosmetics.AddRange(boardTheme, badge);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/cosmetics?type=BoardTheme");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CosmeticsDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Cosmetics.Should().OnlyContain(c => c.Type == "BoardTheme");
    }

    #endregion

    #region GET /api/gamification/goals Tests

    [Fact]
    public async Task GetPersonalGoals_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/gamification/goals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPersonalGoals_ReturnsActiveGoals()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-goals", "goals@test.com", "Goals User", AuthProvider.Microsoft);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var goal = PersonalGoal.Create(
            userId: user.Id,
            type: GoalType.BeatAverage,
            description: "Score 1000 points today",
            targetValue: 1000,
            expiresAt: DateTime.UtcNow.AddDays(1)
        );
        db.PersonalGoals.Add(goal);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.GetAsync("/api/gamification/goals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<PersonalGoalDto>>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Should().NotBeEmpty();
        result.Should().Contain(g => g.Description == "Score 1000 points today");
    }

    #endregion

    #region POST /api/gamification/streak-freeze Tests

    [Fact]
    public async Task UseStreakFreeze_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/gamification/streak-freeze", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UseStreakFreeze_WithTokens_Success()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-freeze", "freeze@test.com", "Freeze User", AuthProvider.Microsoft);
        user.AddStreakFreezeTokens(1); // Give the user a freeze token
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.PostAsync("/api/gamification/streak-freeze", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<UseStreakFreezeResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UseStreakFreeze_NoTokens_ReturnsOkWithSuccessFalse()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-nofreeze", "nofreeze@test.com", "No Freeze User", AuthProvider.Microsoft);
        // User has no freeze tokens
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Act
        var response = await client.PostAsync("/api/gamification/streak-freeze", null);

        // Assert
        // The service returns false (not successful) but not an error
        // So we expect 200 OK with Success = false
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UseStreakFreezeResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    #endregion

    #region POST /api/gamification/equip Tests

    [Fact]
    public async Task EquipCosmetic_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new EquipCosmeticRequest(Guid.NewGuid());

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/equip", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task EquipCosmetic_Owned_Success()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-equip", "equip@test.com", "Equip User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var cosmetic = Cosmetic.Create(
            code: "theme_owned",
            name: "Owned Theme",
            description: "A theme owned by user",
            type: CosmeticType.BoardTheme,
            assetUrl: "/themes/owned.png",
            previewUrl: "/themes/owned-preview.png",
            rarity: CosmeticRarity.Common,
            isDefault: true // Default cosmetics are owned by all
        );
        db.Cosmetics.Add(cosmetic);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        // Use the cosmetic ID as a Guid - convert int to Guid for the request
        var cosmeticGuid = ConvertIdToGuid(cosmetic.Id);
        var request = new EquipCosmeticRequest(cosmeticGuid);

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/equip", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<EquipCosmeticResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task EquipCosmetic_NotOwned_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-notowned", "notowned@test.com", "Not Owned User", AuthProvider.Microsoft);
        db.Users.Add(user);

        var cosmetic = Cosmetic.Create(
            code: "theme_notowned",
            name: "Not Owned Theme",
            description: "A theme not owned by user",
            type: CosmeticType.BoardTheme,
            assetUrl: "/themes/notowned.png",
            previewUrl: "/themes/notowned-preview.png",
            rarity: CosmeticRarity.Legendary,
            isDefault: false // Not default, so user doesn't own it
        );
        db.Cosmetics.Add(cosmetic);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        var cosmeticGuid = ConvertIdToGuid(cosmetic.Id);
        var request = new EquipCosmeticRequest(cosmeticGuid);

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/equip", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<EquipCosmeticResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("own");
    }

    #endregion

    #region POST /api/gamification/claim-reward Tests

    [Fact]
    public async Task ClaimReward_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new ClaimRewardRequest(Guid.NewGuid());

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/claim-reward", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ClaimReward_Unlocked_Success()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-claim", "claim@test.com", "Claim User", AuthProvider.Microsoft);
        user.AddSeasonXP(1000); // Give enough XP to be at tier 1+
        db.Users.Add(user);

        var season = Season.Create(
            name: "Claim Season",
            number: 1,
            startDate: DateTime.UtcNow.AddDays(-10),
            endDate: DateTime.UtcNow.AddDays(80),
            theme: "Test"
        );
        db.Seasons.Add(season);
        await db.SaveChangesAsync();

        var reward = SeasonReward.Create(
            seasonId: season.Id,
            tier: 1,
            rewardType: RewardType.XPBoost,
            rewardValue: 50,
            xpRequired: 100
        );
        db.SeasonRewards.Add(reward);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        var rewardGuid = ConvertIdToGuid(reward.Id);
        var request = new ClaimRewardRequest(rewardGuid);

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/claim-reward", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ClaimRewardResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ClaimReward_NotUnlocked_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-notunlock", "notunlock@test.com", "Not Unlock User", AuthProvider.Microsoft);
        // User has 0 XP, tier 0
        db.Users.Add(user);

        var season = Season.Create(
            name: "Not Unlock Season",
            number: 1,
            startDate: DateTime.UtcNow.AddDays(-10),
            endDate: DateTime.UtcNow.AddDays(80),
            theme: "Test"
        );
        db.Seasons.Add(season);
        await db.SaveChangesAsync();

        var reward = SeasonReward.Create(
            seasonId: season.Id,
            tier: 10, // High tier that user hasn't reached
            rewardType: RewardType.XPBoost,
            rewardValue: 500,
            xpRequired: 10000
        );
        db.SeasonRewards.Add(reward);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");

        var rewardGuid = ConvertIdToGuid(reward.Id);
        var request = new ClaimRewardRequest(rewardGuid);

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/claim-reward", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<ClaimRewardResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
    }

    #endregion

    #region POST /api/gamification/timezone Tests

    [Fact]
    public async Task SetTimezone_Unauthenticated_Returns401()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new SetTimezoneRequest("America/New_York");

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/timezone", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetTimezone_Valid_Success()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-tz", "tz@test.com", "TZ User", AuthProvider.Microsoft);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");
        var request = new SetTimezoneRequest("America/New_York");

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/timezone", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SetTimezoneResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task SetTimezone_Invalid_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-badtz", "badtz@test.com", "Bad TZ User", AuthProvider.Microsoft);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "microsoft");
        var request = new SetTimezoneRequest("Invalid/Timezone");

        // Act
        var response = await client.PostAsJsonAsync("/api/gamification/timezone", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<SetTimezoneResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid timezone");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Converts an integer ID to a Guid using the same logic as the endpoint's GetUserGuid.
    /// The handlers use: Math.Abs(BitConverter.ToInt32(bytes, 0) % 1000000) + 1
    /// So we encode (id - 1) to get the correct id after decoding.
    /// </summary>
    private static Guid ConvertIdToGuid(int id)
    {
        // Encode (id - 1) so that when handler decodes it: Math.Abs((id-1) % 1000000) + 1 = id
        var valueToEncode = id - 1;
        var bytes = new byte[16];
        BitConverter.GetBytes(valueToEncode).CopyTo(bytes, 0);
        return new Guid(bytes);
    }

    #endregion
}
