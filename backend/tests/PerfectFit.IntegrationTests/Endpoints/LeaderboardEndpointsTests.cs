using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data;
using PerfectFit.Web.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PerfectFit.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for leaderboard API endpoints.
/// Each test creates its own factory instance to ensure database isolation.
/// </summary>
public class LeaderboardEndpointsTests
{
    [Fact]
    public async Task GetTopScores_ReturnsEmptyList_WhenNoScores()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/leaderboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var scores = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
        scores.Should().NotBeNull();
        scores.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTopScores_ReturnsOrderedList_WhenScoresExist()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test users
        var user1 = User.Create("ext-1", "user1@test.com", "Player One", AuthProvider.Google);
        var user2 = User.Create("ext-2", "user2@test.com", "Player Two", AuthProvider.Google);
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        // Create game sessions
        var session1 = GameSession.Create(user1.Id);
        var session2 = GameSession.Create(user2.Id);
        session1.AddScore(100, 5);
        session1.EndGame();
        session2.AddScore(200, 10);
        session2.EndGame();
        db.GameSessions.AddRange(session1, session2);
        await db.SaveChangesAsync();

        // Create leaderboard entries
        var entry1 = LeaderboardEntry.Create(user1.Id, 100, 5, 2, session1.Id);
        var entry2 = LeaderboardEntry.Create(user2.Id, 200, 10, 4, session2.Id);
        db.LeaderboardEntries.AddRange(entry1, entry2);
        await db.SaveChangesAsync();

        // Act
        var response = await client.GetAsync("/api/leaderboard");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var scores = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
        scores.Should().NotBeNull();
        scores.Should().HaveCount(2);
        scores![0].Score.Should().Be(200); // Higher score first
        scores[0].Rank.Should().Be(1);
        scores[1].Score.Should().Be(100);
        scores[1].Rank.Should().Be(2);
    }

    [Fact]
    public async Task GetTopScores_LimitsResults_WhenCountProvided()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Create test user
        var user = User.Create("ext-limit", "limit@test.com", "Limit User", AuthProvider.Google);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        // Create multiple game sessions and entries
        for (int i = 0; i < 5; i++)
        {
            var session = GameSession.Create(user.Id);
            session.AddScore((i + 1) * 100, i + 1);
            session.EndGame();
            db.GameSessions.Add(session);
            await db.SaveChangesAsync();

            var entry = LeaderboardEntry.Create(user.Id, (i + 1) * 100, i + 1, i, session.Id);
            db.LeaderboardEntries.Add(entry);
            await db.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/leaderboard?count=3");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var scores = await response.Content.ReadFromJsonAsync<List<LeaderboardEntryDto>>();
        scores.Should().NotBeNull();
        scores.Should().HaveCount(3);
        scores![0].Score.Should().Be(500); // Highest score
    }

    [Fact]
    public async Task GetMyStats_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/leaderboard/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyStats_ReturnsCorrectData_WhenAuthenticated()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-stats", "stats@test.com", "Stats User", AuthProvider.Google);
        user.IncrementGamesPlayed();
        user.IncrementGamesPlayed();
        user.UpdateHighScore(500);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var session = GameSession.Create(user.Id);
        session.AddScore(500, 20);
        session.UpdateCombo(5);
        session.EndGame();
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        var entry = LeaderboardEntry.Create(user.Id, 500, 20, 5, session.Id);
        db.LeaderboardEntries.Add(entry);
        await db.SaveChangesAsync();

        // Create authenticated client
        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "google");

        // Act
        var response = await client.GetAsync("/api/leaderboard/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var stats = await response.Content.ReadFromJsonAsync<UserStatsDto>();
        stats.Should().NotBeNull();
        stats!.HighScore.Should().Be(500);
        stats.GamesPlayed.Should().Be(2);
        stats.GlobalRank.Should().Be(1);
        stats.BestGame.Should().NotBeNull();
        stats.BestGame!.Score.Should().Be(500);
    }

    [Fact]
    public async Task SubmitScore_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();
        var request = new SubmitScoreRequestDto(Guid.NewGuid());

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubmitScore_ValidGame_CreatesEntry()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-submit", "submit@test.com", "Submit User", AuthProvider.Google);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var session = GameSession.Create(user.Id);
        session.AddScore(300, 15);
        session.UpdateCombo(3);
        session.EndGame();
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "google");
        var request = new SubmitScoreRequestDto(session.Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Entry.Should().NotBeNull();
        result.Entry!.Score.Should().Be(300);
        result.IsNewHighScore.Should().BeTrue();
        result.NewRank.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SubmitScore_InvalidGame_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-invalid", "invalid@test.com", "Invalid User", AuthProvider.Google);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "google");
        var request = new SubmitScoreRequestDto(Guid.NewGuid()); // Non-existent game

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task SubmitScore_GuestUser_ReturnsForbidden()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var guestUser = User.Create("guest-123", null, "Guest_ABC123", AuthProvider.Guest);
        db.Users.Add(guestUser);
        await db.SaveChangesAsync();

        var session = GameSession.Create(guestUser.Id);
        session.AddScore(100, 5);
        session.EndGame();
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(guestUser.Id, guestUser.DisplayName, "guest");
        var request = new SubmitScoreRequestDto(session.Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Guest");
    }

    [Fact]
    public async Task SubmitScore_GameNotEnded_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-notended", "notended@test.com", "NotEnded User", AuthProvider.Google);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var session = GameSession.Create(user.Id);
        session.AddScore(100, 5);
        // NOT calling EndGame()
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "google");
        var request = new SubmitScoreRequestDto(session.Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not ended");
    }

    [Fact]
    public async Task SubmitScore_AlreadySubmitted_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = User.Create("ext-dup", "dup@test.com", "Dup User", AuthProvider.Google);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var session = GameSession.Create(user.Id);
        session.AddScore(200, 10);
        session.EndGame();
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        // Already submitted
        var existingEntry = LeaderboardEntry.Create(user.Id, 200, 10, 2, session.Id);
        db.LeaderboardEntries.Add(existingEntry);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient(user.Id, user.DisplayName, "google");
        var request = new SubmitScoreRequestDto(session.Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already");
    }

    [Fact]
    public async Task SubmitScore_OtherUsersGame_ReturnsBadRequest()
    {
        // Arrange
        await using var factory = new CustomWebApplicationFactory();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user1 = User.Create("ext-owner", "owner@test.com", "Owner User", AuthProvider.Google);
        var user2 = User.Create("ext-other", "other@test.com", "Other User", AuthProvider.Google);
        db.Users.AddRange(user1, user2);
        await db.SaveChangesAsync();

        var session = GameSession.Create(user1.Id); // Owned by user1
        session.AddScore(200, 10);
        session.EndGame();
        db.GameSessions.Add(session);
        await db.SaveChangesAsync();

        // user2 tries to submit user1's game
        var client = factory.CreateAuthenticatedClient(user2.Id, user2.DisplayName, "google");
        var request = new SubmitScoreRequestDto(session.Id);

        // Act
        var response = await client.PostAsJsonAsync("/api/leaderboard/submit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var result = await response.Content.ReadFromJsonAsync<SubmitScoreResponseDto>();
        result.Should().NotBeNull();
        result!.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("does not belong");
    }
}
