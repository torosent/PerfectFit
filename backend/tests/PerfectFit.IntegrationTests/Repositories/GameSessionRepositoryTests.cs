using FluentAssertions;
using PerfectFit.Core.Entities;
using PerfectFit.Core.Enums;
using PerfectFit.Infrastructure.Data.Repositories;

namespace PerfectFit.IntegrationTests.Repositories;

public class GameSessionRepositoryTests : RepositoryTestBase
{
    private readonly GameSessionRepository _repository;

    public GameSessionRepositoryTests()
    {
        _repository = new GameSessionRepository(DbContext);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistGameSession()
    {
        // Arrange
        var session = GameSession.Create(null);

        // Act
        var result = await _repository.AddAsync(session);

        // Assert
        result.Id.Should().NotBeEmpty();

        var savedSession = await DbContext.GameSessions.FindAsync(result.Id);
        savedSession.Should().NotBeNull();
        savedSession!.Status.Should().Be(GameStatus.Playing);
        savedSession.Score.Should().Be(0);
    }

    [Fact]
    public async Task AddAsync_WithUserId_ShouldPersistWithUserId()
    {
        // Arrange
        var user = User.Create("test-user", "test@example.com", "Test", AuthProvider.Google);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var session = GameSession.Create(user.Id);

        // Act
        var result = await _repository.AddAsync(session);

        // Assert
        result.UserId.Should().Be(user.Id);

        var savedSession = await DbContext.GameSessions.FindAsync(result.Id);
        savedSession!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionExists_ShouldReturnSession()
    {
        // Arrange
        var session = GameSession.Create(null);
        await _repository.AddAsync(session);

        // Act
        var result = await _repository.GetByIdAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var session = GameSession.Create(null);
        await _repository.AddAsync(session);

        session.AddScore(100, 2);
        session.UpdateCombo(3);

        // Act
        await _repository.UpdateAsync(session);

        // Assert
        var updatedSession = await DbContext.GameSessions.FindAsync(session.Id);
        updatedSession.Should().NotBeNull();
        updatedSession!.Score.Should().Be(100);
        updatedSession.LinesCleared.Should().Be(2);
        updatedSession.Combo.Should().Be(3);
        updatedSession.MaxCombo.Should().Be(3);
    }

    [Fact]
    public async Task GetActiveSessionsByUserIdAsync_ShouldReturnOnlyActiveSessions()
    {
        // Arrange
        var user = User.Create("session-user", "test@example.com", "Test", AuthProvider.Google);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var activeSession1 = GameSession.Create(user.Id);
        var activeSession2 = GameSession.Create(user.Id);
        var endedSession = GameSession.Create(user.Id);
        endedSession.EndGame();

        await _repository.AddAsync(activeSession1);
        await _repository.AddAsync(activeSession2);
        await _repository.AddAsync(endedSession);

        // Act
        var result = await _repository.GetActiveSessionsByUserIdAsync(user.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.Status.Should().Be(GameStatus.Playing));
    }

    [Fact]
    public async Task GetActiveSessionsByUserIdAsync_WhenNoActiveSessions_ShouldReturnEmpty()
    {
        // Arrange
        var user = User.Create("no-active-user", "test@example.com", "Test", AuthProvider.Google);
        DbContext.Users.Add(user);
        await DbContext.SaveChangesAsync();

        var endedSession = GameSession.Create(user.Id);
        endedSession.EndGame();
        await _repository.AddAsync(endedSession);

        // Act
        var result = await _repository.GetActiveSessionsByUserIdAsync(user.Id);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_BoardState_ShouldPersistJsonState()
    {
        // Arrange
        var session = GameSession.Create(null);
        await _repository.AddAsync(session);

        var newBoardState = """{"grid":[[1,1,0,0,0,0,0,0,0,0]]}""";
        var newPieces = """[{"type":"T","rotation":0}]""";
        var newBag = """{"index":7,"pieces":["I","O","T"]}""";

        session.UpdateBoard(newBoardState, newPieces, newBag);

        // Act
        await _repository.UpdateAsync(session);

        // Assert
        var updatedSession = await DbContext.GameSessions.FindAsync(session.Id);
        updatedSession!.BoardState.Should().Be(newBoardState);
        updatedSession.CurrentPieces.Should().Be(newPieces);
        updatedSession.PieceBagState.Should().Be(newBag);
    }
}
