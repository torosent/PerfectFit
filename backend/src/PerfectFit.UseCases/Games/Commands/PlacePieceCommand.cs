using MediatR;
using PerfectFit.Core.Enums;
using PerfectFit.Core.GameLogic;
using PerfectFit.Core.GameLogic.Pieces;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Games.DTOs;
using System.Text.Json;

namespace PerfectFit.UseCases.Games.Commands;

/// <summary>
/// Command to place a piece on the game board.
/// </summary>
/// <param name="GameId">The ID of the game.</param>
/// <param name="PieceIndex">Index of the piece to place (0-2).</param>
/// <param name="Row">Row position on the board.</param>
/// <param name="Col">Column position on the board.</param>
/// <param name="ClientTimestamp">Optional client timestamp for timing validation.</param>
public record PlacePieceCommand(
    Guid GameId,
    int? UserId,
    int PieceIndex,
    int Row,
    int Col,
    long? ClientTimestamp = null
) : IRequest<PlacePieceResult>;

/// <summary>
/// Result of the place piece operation.
/// </summary>
public record PlacePieceResult(
    bool Found,
    bool GameActive,
    PlacePieceResponseDto? Response,
    string? RejectionReason = null
);

/// <summary>
/// Handler for placing a piece on the game board.
/// </summary>
public class PlacePieceCommandHandler : IRequestHandler<PlacePieceCommand, PlacePieceResult>
{
    private readonly IGameSessionRepository _gameSessionRepository;

    // Anti-cheat constants
    private const int MinTimeBetweenMovesMs = 50;    // Minimum 50ms between moves (human reaction time)
    private const int MaxMovesPerGame = 500;          // Maximum moves per game (prevents infinite games)
    private const int MaxGameDurationHours = 24;      // Maximum game duration

    public PlacePieceCommandHandler(IGameSessionRepository gameSessionRepository)
    {
        _gameSessionRepository = gameSessionRepository;
    }

    public async Task<PlacePieceResult> Handle(PlacePieceCommand request, CancellationToken cancellationToken)
    {
        var session = await _gameSessionRepository.GetByIdAsync(request.GameId, cancellationToken);

        if (session is null)
        {
            return new PlacePieceResult(Found: false, GameActive: false, Response: null);
        }

        // Verify session ownership if user is authenticated
        if (request.UserId.HasValue && session.UserId != request.UserId.Value)
        {
            return new PlacePieceResult(
                Found: true,
                GameActive: true,
                Response: null,
                RejectionReason: "Game session does not belong to this user");
        }

        if (session.Status != GameStatus.Playing)
        {
            return new PlacePieceResult(Found: true, GameActive: false, Response: null);
        }

        // Anti-cheat: Check game duration
        var gameDuration = session.GetGameDuration();
        if (gameDuration > MaxGameDurationHours * 3600)
        {
            session.EndGame();
            await _gameSessionRepository.UpdateAsync(session, cancellationToken);
            return new PlacePieceResult(
                Found: true,
                GameActive: false,
                Response: null,
                RejectionReason: "Game exceeded maximum duration");
        }

        // Anti-cheat: Check move count
        if (session.MoveCount >= MaxMovesPerGame)
        {
            return new PlacePieceResult(
                Found: true,
                GameActive: true,
                Response: null,
                RejectionReason: "Maximum moves reached");
        }

        // Anti-cheat: Rate limiting - check time since last move
        var timeSinceLastMove = session.GetTimeSinceLastMove();
        if (timeSinceLastMove.HasValue && timeSinceLastMove.Value < MinTimeBetweenMovesMs)
        {
            return new PlacePieceResult(
                Found: true,
                GameActive: true,
                Response: null,
                RejectionReason: "Move rate limit exceeded");
        }

        // Validate piece index bounds before game logic
        if (request.PieceIndex < 0 || request.PieceIndex > 2)
        {
            return new PlacePieceResult(
                Found: true,
                GameActive: true,
                Response: null,
                RejectionReason: "Invalid piece index");
        }

        // Validate board position bounds
        if (request.Row < 0 || request.Row > 7 || request.Col < 0 || request.Col > 7)
        {
            return new PlacePieceResult(
                Found: true,
                GameActive: true,
                Response: null,
                RejectionReason: "Invalid board position");
        }

        // Reconstruct game engine from saved state
        var gameState = DeserializeGameState(session);
        var engine = GameEngine.FromState(gameState);

        // Attempt to place piece
        var placementResult = engine.PlacePiece(request.PieceIndex, request.Row, request.Col);

        if (!placementResult.Success)
        {
            // Invalid placement
            return new PlacePieceResult(
                Found: true,
                GameActive: true,
                Response: new PlacePieceResponseDto(
                    Success: false,
                    GameState: MapToDto(session, engine),
                    LinesCleared: 0,
                    PointsEarned: 0,
                    IsGameOver: engine.IsGameOver,
                    PiecesRemainingInTurn: engine.CurrentPieces.Count,
                    NewTurnStarted: false
                )
            );
        }

        // Record the move for anti-cheat tracking
        session.RecordMove(
            request.PieceIndex,
            request.Row,
            request.Col,
            placementResult.PointsEarned,
            placementResult.LinesCleared);

        // Update session with new state
        var newState = engine.GetState();
        var boardState = SerializeBoardState(newState.BoardGrid);
        var piecesJson = SerializePieces(newState.CurrentPieces);

        session.UpdateBoard(boardState, piecesJson, newState.PieceBagState);
        session.AddScore(placementResult.PointsEarned, placementResult.LinesCleared);
        session.UpdateCombo(placementResult.NewCombo);

        if (placementResult.IsGameOver)
        {
            session.EndGame();
        }

        await _gameSessionRepository.UpdateAsync(session, cancellationToken);

        return new PlacePieceResult(
            Found: true,
            GameActive: true,
            Response: new PlacePieceResponseDto(
                Success: true,
                GameState: MapToDto(session, engine),
                LinesCleared: placementResult.LinesCleared,
                PointsEarned: placementResult.PointsEarned,
                IsGameOver: placementResult.IsGameOver,
                PiecesRemainingInTurn: placementResult.PiecesRemainingInTurn,
                NewTurnStarted: placementResult.NewTurnStarted
            )
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

    private static string SerializeBoardState(string?[,] grid)
    {
        var rows = grid.GetLength(0);
        var cols = grid.GetLength(1);
        var gridArray = new string?[rows][];

        for (int i = 0; i < rows; i++)
        {
            gridArray[i] = new string?[cols];
            for (int j = 0; j < cols; j++)
            {
                gridArray[i][j] = grid[i, j];
            }
        }

        return JsonSerializer.Serialize(new { grid = gridArray });
    }

    private static string SerializePieces(List<PieceInfo> pieces)
    {
        return JsonSerializer.Serialize(pieces);
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
