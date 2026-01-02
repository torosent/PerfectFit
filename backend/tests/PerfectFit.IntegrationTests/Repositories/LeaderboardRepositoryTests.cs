using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data.Repositories;

namespace PerfectFit.IntegrationTests.Repositories;

public class LeaderboardRepositoryTests : RepositoryTestBase
{
    private readonly LeaderboardRepository _repository;

    public LeaderboardRepositoryTests()
    {
        _repository = new LeaderboardRepository(DbContext);
    }

    private async Task<(User user, GameSession session)> CreateUserAndSessionAsync(string externalId, string displayName)
    {
        var user = User.Create(externalId, $"{externalId}@example.com", displayName, AuthProvider.Google);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var session = GameSession.Create(user.Id);
        DbContext.GameSessions.Add(session);
        await DbContext.SaveChangesAsync();

        return (user, session);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistLeaderboardEntry()
    {
        // Arrange
        var (user, session) = await CreateUserAndSessionAsync("leaderboard-user", "Leader");
        var entry = LeaderboardEntry.Create(user.Id, 1000, 50, 5, session.Id);

        // Act
        var result = await _repository.AddAsync(entry);

        // Assert
        result.Id.Should().BeGreaterThan(0);
        
        var savedEntry = await DbContext.LeaderboardEntries.FindAsync(result.Id);
        savedEntry.Should().NotBeNull();
        savedEntry!.Score.Should().Be(1000);
        savedEntry.LinesCleared.Should().Be(50);
        savedEntry.MaxCombo.Should().Be(5);
    }

    [Fact]
    public async Task GetTopScoresAsync_ShouldReturnTopScoresDescending()
    {
        // Arrange
        var (user1, session1) = await CreateUserAndSessionAsync("user1", "User One");
        var (user2, session2) = await CreateUserAndSessionAsync("user2", "User Two");
        var (user3, session3) = await CreateUserAndSessionAsync("user3", "User Three");

        await _repository.AddAsync(LeaderboardEntry.Create(user1.Id, 500, 20, 3, session1.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user2.Id, 1000, 40, 5, session2.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user3.Id, 750, 30, 4, session3.Id));

        // Act
        var result = (await _repository.GetTopScoresAsync(10)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Score.Should().Be(1000);
        result[1].Score.Should().Be(750);
        result[2].Score.Should().Be(500);
    }

    [Fact]
    public async Task GetTopScoresAsync_ShouldRespectCountLimit()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var (user, session) = await CreateUserAndSessionAsync($"user-{i}", $"User {i}");
            await _repository.AddAsync(LeaderboardEntry.Create(user.Id, (i + 1) * 100, 10, 2, session.Id));
        }

        // Act
        var result = await _repository.GetTopScoresAsync(3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTopScoresAsync_WhenEmpty_ShouldReturnEmptyCollection()
    {
        // Act
        var result = await _repository.GetTopScoresAsync(10);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserBestScoreAsync_ShouldReturnHighestScoreForUser()
    {
        // Arrange
        var (user, session1) = await CreateUserAndSessionAsync("best-score-user", "Best User");
        var session2 = GameSession.Create(user.Id);
        var session3 = GameSession.Create(user.Id);
        DbContext.GameSessions.AddRange(session2, session3);
        await DbContext.SaveChangesAsync();

        await _repository.AddAsync(LeaderboardEntry.Create(user.Id, 500, 20, 3, session1.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user.Id, 1500, 60, 7, session2.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user.Id, 800, 35, 4, session3.Id));

        // Act
        var result = await _repository.GetUserBestScoreAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Score.Should().Be(1500);
    }

    [Fact]
    public async Task GetUserBestScoreAsync_WhenUserHasNoScores_ShouldReturnNull()
    {
        // Arrange
        var user = User.Create("no-scores-user", "test@example.com", "No Scores", AuthProvider.Google);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserBestScoreAsync(user.Id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserRankAsync_ShouldReturnCorrectRank()
    {
        // Arrange
        var (user1, session1) = await CreateUserAndSessionAsync("rank-user1", "Rank User 1");
        var (user2, session2) = await CreateUserAndSessionAsync("rank-user2", "Rank User 2");
        var (user3, session3) = await CreateUserAndSessionAsync("rank-user3", "Rank User 3");

        await _repository.AddAsync(LeaderboardEntry.Create(user1.Id, 1000, 40, 5, session1.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user2.Id, 500, 20, 3, session2.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user3.Id, 750, 30, 4, session3.Id));

        // Act
        var rank1 = await _repository.GetUserRankAsync(user1.Id);
        var rank2 = await _repository.GetUserRankAsync(user2.Id);
        var rank3 = await _repository.GetUserRankAsync(user3.Id);

        // Assert
        rank1.Should().Be(1);  // Highest score
        rank2.Should().Be(3);  // Lowest score
        rank3.Should().Be(2);  // Middle score
    }

    [Fact]
    public async Task GetUserRankAsync_WhenUserHasNoScores_ShouldReturnZero()
    {
        // Arrange
        var user = User.Create("unranked-user", "test@example.com", "Unranked", AuthProvider.Google);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserRankAsync(user.Id);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task GetUserRankAsync_WithMultipleEntriesSameUser_ShouldUseHighestScore()
    {
        // Arrange
        var (user1, session1a) = await CreateUserAndSessionAsync("multi-score-user1", "Multi User 1");
        var session1b = GameSession.Create(user1.Id);
        DbContext.GameSessions.Add(session1b);
        await DbContext.SaveChangesAsync();

        var (user2, session2) = await CreateUserAndSessionAsync("multi-score-user2", "Multi User 2");

        // User1 has two entries: 500 and 1000 (best is 1000)
        await _repository.AddAsync(LeaderboardEntry.Create(user1.Id, 500, 20, 3, session1a.Id));
        await _repository.AddAsync(LeaderboardEntry.Create(user1.Id, 1000, 40, 5, session1b.Id));
        
        // User2 has one entry: 750
        await _repository.AddAsync(LeaderboardEntry.Create(user2.Id, 750, 30, 4, session2.Id));

        // Act
        var rank1 = await _repository.GetUserRankAsync(user1.Id);
        var rank2 = await _repository.GetUserRankAsync(user2.Id);

        // Assert
        rank1.Should().Be(1);  // User1's best (1000) is highest
        rank2.Should().Be(2);  // User2's best (750) is second
    }
}
