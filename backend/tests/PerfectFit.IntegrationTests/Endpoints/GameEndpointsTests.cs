using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using PerfectFit.UseCases.Games.DTOs;
using System.Net;
using System.Net.Http.Json;

// Web DTOs for game end response
using GameEndResponseWebDto = PerfectFit.Web.DTOs.GameEndResponseWebDto;
using GameStatusWebDto = PerfectFit.Web.DTOs.GameStatusWebDto;

namespace PerfectFit.IntegrationTests.Endpoints;

/// <summary>
/// Integration tests for game API endpoints.
/// </summary>
public class GameEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GameEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGame_ReturnsNewGameState()
    {
        // Act
        var response = await _client.PostAsync("/api/games", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var gameState = await response.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);
        gameState.Should().NotBeNull();
        gameState!.Id.Should().NotBeNullOrEmpty();
        gameState.Score.Should().Be(0);
        gameState.Combo.Should().Be(0);
        gameState.Status.Should().Be(GameStatusDto.Playing);
        gameState.CurrentPieces.Should().HaveCount(3);
        gameState.Grid.Should().HaveCount(8); // 8 rows
        gameState.LinesCleared.Should().Be(0);
    }

    [Fact]
    public async Task GetGame_ReturnsExistingGame()
    {
        // Arrange - Create a game first
        var createResponse = await _client.PostAsync("/api/games", null);
        var createdGame = await createResponse.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);

        // Act
        var response = await _client.GetAsync($"/api/games/{createdGame!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameState = await response.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);
        gameState.Should().NotBeNull();
        gameState!.Id.Should().Be(createdGame.Id);
    }

    [Fact]
    public async Task GetGame_Returns404_WhenNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/games/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlacePiece_ValidPlacement_ReturnsUpdatedState()
    {
        // Arrange - Create a game first
        var createResponse = await _client.PostAsync("/api/games", null);
        var createdGame = await createResponse.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);

        var request = new PlacePieceRequestDto(
            PieceIndex: 0,
            Position: new PositionDto(Row: 0, Col: 0)
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{createdGame!.Id}/place",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PlacePieceResponseDto>(CustomWebApplicationFactory.JsonOptions);
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.GameState.Should().NotBeNull();
        // After placing piece 0, we should have 2 pieces (new pieces drawn only after all 3 placed)
        result.GameState.CurrentPieces.Should().HaveCount(2);
        result.PiecesRemainingInTurn.Should().Be(2);
        result.NewTurnStarted.Should().BeFalse();
    }

    [Fact]
    public async Task PlacePiece_InvalidPieceIndex_ReturnsBadRequest()
    {
        // Arrange - Create a game first
        var createResponse = await _client.PostAsync("/api/games", null);
        var createdGame = await createResponse.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);

        var request = new PlacePieceRequestDto(
            PieceIndex: 10, // Invalid index
            Position: new PositionDto(Row: 0, Col: 0)
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{createdGame!.Id}/place",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PlacePiece_GameNotFound_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new PlacePieceRequestDto(
            PieceIndex: 0,
            Position: new PositionDto(Row: 0, Col: 0)
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{nonExistentId}/place",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task EndGame_MarksGameAsEnded()
    {
        // Arrange - Create a game first
        var createResponse = await _client.PostAsync("/api/games", null);
        var createdGame = await createResponse.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);

        // Act
        var response = await _client.PostAsync(
            $"/api/games/{createdGame!.Id}/end",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var endResponse = await response.Content.ReadFromJsonAsync<GameEndResponseWebDto>(CustomWebApplicationFactory.JsonOptions);
        endResponse.Should().NotBeNull();
        endResponse!.GameState.Should().NotBeNull();
        endResponse.GameState.Status.Should().Be(GameStatusWebDto.Ended);
        // Gamification should be null for unauthenticated users
        endResponse.Gamification.Should().BeNull();
    }

    [Fact]
    public async Task EndGame_GameNotFound_Returns404()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync(
            $"/api/games/{nonExistentId}/end",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PlacePiece_OnEndedGame_ReturnsBadRequest()
    {
        // Arrange - Create and end a game
        var createResponse = await _client.PostAsync("/api/games", null);
        var createdGame = await createResponse.Content.ReadFromJsonAsync<GameStateDto>(CustomWebApplicationFactory.JsonOptions);
        await _client.PostAsync($"/api/games/{createdGame!.Id}/end", null);

        var request = new PlacePieceRequestDto(
            PieceIndex: 0,
            Position: new PositionDto(Row: 0, Col: 0)
        );

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/games/{createdGame.Id}/place",
            request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
