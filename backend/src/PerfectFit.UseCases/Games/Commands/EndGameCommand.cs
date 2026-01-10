using MediatR;
using Microsoft.Extensions.Logging;
using PerfectFit.Core.Enums;
using PerfectFit.Core.GameLogic;
using PerfectFit.Core.GameLogic.Pieces;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Gamification;
using PerfectFit.UseCases.Gamification.Commands;
using PerfectFit.UseCases.Games.DTOs;
using System.Text.Json;

namespace PerfectFit.UseCases.Games.Commands;

/// <summary>
/// Command to end an active game.
/// </summary>
/// <param name="GameId">The ID of the game to end.</param>
/// <param name="UserId">Optional user ID for gamification processing.</param>
public record EndGameCommand(Guid GameId, int? UserId = null) : IRequest<EndGameResult>;

/// <summary>
/// Result of the end game operation.
/// </summary>
public record EndGameResult(bool Found, GameEndResponseDto? Response);

/// <summary>
/// Handler for ending a game session.
/// </summary>
public class EndGameCommandHandler : IRequestHandler<EndGameCommand, EndGameResult>
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<EndGameCommandHandler> _logger;

    public EndGameCommandHandler(
        IGameSessionRepository gameSessionRepository,
        IMediator mediator,
        ILogger<EndGameCommandHandler> logger)
    {
        _gameSessionRepository = gameSessionRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<EndGameResult> Handle(EndGameCommand request, CancellationToken cancellationToken)
    {
        var session = await _gameSessionRepository.GetByIdAsync(request.GameId, cancellationToken);

        if (session is null)
        {
            return new EndGameResult(Found: false, Response: null);
        }

        // End the game (idempotent - won't throw if already ended)
        session.EndGame();
        await _gameSessionRepository.UpdateAsync(session, cancellationToken);

        // Reconstruct game engine for DTO
        var gameState = DeserializeGameState(session);
        var engine = GameEngine.FromState(gameState);
        var gameStateDto = MapToDto(session, engine);

        // Process gamification if user is authenticated
        GameEndGamificationResponseDto? gamificationDto = null;
        if (request.UserId.HasValue && session.UserId.HasValue && session.UserId == request.UserId)
        {
            try
            {
                var gamificationCommand = new ProcessGameEndGamificationCommand(request.UserId.Value, request.GameId);
                var gamificationResult = await _mediator.Send(gamificationCommand, cancellationToken);
                gamificationDto = MapGamificationToDto(gamificationResult);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the game end
                // Gamification is optional enhancement
                _logger.LogError(ex, "Failed to process gamification for user {UserId} and game {GameId}", 
                    request.UserId, request.GameId);
            }
        }

        return new EndGameResult(Found: true, Response: new GameEndResponseDto(gameStateDto, gamificationDto));
    }

    private static GameEndGamificationResponseDto MapGamificationToDto(GameEndGamificationResult result)
    {
        return new GameEndGamificationResponseDto(
            Streak: new StreakResponseDto(
                CurrentStreak: result.Streak.NewStreak,
                LongestStreak: result.Streak.LongestStreak,
                FreezeTokens: null, // Not available in StreakResult - fetch from user status endpoint for full details
                IsAtRisk: null, // Not available in StreakResult - determined by streak service based on time
                ResetTime: null // Not available in StreakResult - fetch from user status endpoint for full details
            ),
            ChallengeUpdates: result.ChallengeUpdates.Select(c => new ChallengeProgressResponseDto(
                ChallengeId: c.ChallengeId,
                ChallengeName: c.ChallengeName,
                NewProgress: c.NewProgress,
                JustCompleted: c.IsCompleted,
                XPEarned: c.IsCompleted ? c.XPEarned : null
            )).ToList(),
            NewAchievements: result.AchievementUpdates.UnlockedAchievements.Select(a => new AchievementUnlockResponseDto(
                AchievementId: a.Id,
                Name: a.Name,
                Description: a.Description,
                IconUrl: a.IconUrl,
                RewardType: a.RewardType.ToString(),
                RewardValue: a.RewardValue
            )).ToList(),
            SeasonProgress: new SeasonXPResponseDto(
                XPEarned: result.SeasonProgress.XPEarned,
                TotalXP: result.SeasonProgress.NewXP,
                NewTier: result.SeasonProgress.NewTier,
                TierUp: result.SeasonProgress.TierUp,
                NewRewardsCount: result.SeasonProgress.RewardsAvailable
            ),
            GoalUpdates: result.GoalUpdates.Select(g => new GoalProgressResponseDto(
                GoalId: g.GoalId,
                Description: g.Description,
                NewProgress: g.NewProgress,
                JustCompleted: g.IsCompleted
            )).ToList(),
            GamesPlayed: result.GamesPlayed,
            HighScore: result.HighScore
        );
    }

    private record StoredPiece(PieceType Type, int Rotation);

    private static GameState DeserializeGameState(Core.Entities.GameSession session)
    {
        // Deserialize board state
        var boardDoc = JsonDocument.Parse(session.BoardState);
        var gridElement = boardDoc.RootElement.GetProperty("grid");
        var grid = DeserializeGrid(gridElement);

        // Deserialize pieces
        var pieces = new List<PieceInfo>();
        try
        {
            // Try new format (array of objects)
            var storedPieces = JsonSerializer.Deserialize<StoredPiece[]>(session.CurrentPieces);
            if (storedPieces != null)
            {
                pieces = storedPieces.Select(p => new PieceInfo(p.Type, p.Rotation)).ToList();
            }
        }
        catch (JsonException)
        {
            // Fallback to old format (array of strings)
            try 
            {
                var pieceTypes = JsonSerializer.Deserialize<string[]>(session.CurrentPieces) ?? [];
                pieces = pieceTypes.Select(p => Enum.TryParse<PieceType>(p, out var type) 
                        ? new PieceInfo(type, 0) 
                        : new PieceInfo(PieceType.Dot, 0)).ToList();
            }
            catch
            {
                // If all else fails, empty list
                pieces = [];
            }
        }

        return new GameState(
            BoardGrid: grid,
            CurrentPieces: pieces,
            PieceBagState: session.PieceBagState,
            Score: session.Score,
            Combo: session.Combo,
            TotalLinesCleared: session.LinesCleared,
            MaxCombo: session.MaxCombo
        );
    }

    private static string?[,] DeserializeGrid(JsonElement gridElement)
    {
        var rows = gridElement.GetArrayLength();
        var grid = new string?[rows, 8];

        for (int i = 0; i < rows; i++)
        {
            var row = gridElement[i];
            var cols = row.GetArrayLength();
            for (int j = 0; j < cols; j++)
            {
                var cell = row[j];
                grid[i, j] = cell.ValueKind == JsonValueKind.Null ||
                             cell.ValueKind == JsonValueKind.Number && cell.GetInt32() == 0
                    ? null
                    : cell.GetString();
            }
        }

        return grid;
    }

    private static GameStateDto MapToDto(Core.Entities.GameSession session, GameEngine engine)
    {
        var state = engine.GetState();

        return new GameStateDto(
            Id: session.Id.ToString(),
            Grid: ConvertGrid(state.BoardGrid),
            CurrentPieces: state.CurrentPieces.Select(MapPieceToDto).ToArray(),
            Score: session.Score,
            Combo: session.Combo,
            Status: session.Status == GameStatus.Playing
                ? GameStatusDto.Playing
                : GameStatusDto.Ended,
            LinesCleared: session.LinesCleared
        );
    }

    private static string?[][] ConvertGrid(string?[,] grid)
    {
        var rows = grid.GetLength(0);
        var cols = grid.GetLength(1);
        var result = new string?[rows][];

        for (int i = 0; i < rows; i++)
        {
            result[i] = new string?[cols];
            for (int j = 0; j < cols; j++)
            {
                result[i][j] = grid[i, j];
            }
        }

        return result;
    }

    private static PieceDto MapPieceToDto(PieceInfo pieceInfo)
    {
        var piece = Piece.Create(pieceInfo.Type, pieceInfo.Rotation);
        var shape = ConvertShapeToArray(piece.Shape);

        return new PieceDto(
            Type: MapPieceType(pieceInfo.Type),
            Shape: shape,
            Color: piece.Color
        );
    }

    private static int[][] ConvertShapeToArray(bool[,] shape)
    {
        var rows = shape.GetLength(0);
        var cols = shape.GetLength(1);
        var result = new int[rows][];

        for (int i = 0; i < rows; i++)
        {
            result[i] = new int[cols];
            for (int j = 0; j < cols; j++)
            {
                result[i][j] = shape[i, j] ? 1 : 0;
            }
        }

        return result;
    }

    private static PieceTypeDto MapPieceType(PieceType pieceType) => pieceType switch
    {
        PieceType.I => PieceTypeDto.I,
        PieceType.O => PieceTypeDto.O,
        PieceType.T => PieceTypeDto.T,
        PieceType.S => PieceTypeDto.S,
        PieceType.Z => PieceTypeDto.Z,
        PieceType.J => PieceTypeDto.J,
        PieceType.L => PieceTypeDto.L,
        PieceType.Dot => PieceTypeDto.DOT,
        PieceType.Line2 => PieceTypeDto.LINE2,
        PieceType.Line3 => PieceTypeDto.LINE3,
        PieceType.Line5 => PieceTypeDto.LINE5,
        PieceType.Corner => PieceTypeDto.CORNER,
        PieceType.BigCorner => PieceTypeDto.BIG_CORNER,
        PieceType.Square2x2 => PieceTypeDto.SQUARE_2X2,
        PieceType.Square3x3 => PieceTypeDto.SQUARE_3X3,
        PieceType.Rect2x3 => PieceTypeDto.RECT_2X3,
        _ => throw new ArgumentOutOfRangeException(nameof(pieceType))
    };
}
