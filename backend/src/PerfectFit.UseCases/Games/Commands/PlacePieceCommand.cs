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
public record PlacePieceCommand(Guid GameId, int PieceIndex, int Row, int Col) : IRequest<PlacePieceResult>;

/// <summary>
/// Result of the place piece operation.
/// </summary>
public record PlacePieceResult(
    bool Found,
    bool GameActive,
    PlacePieceResponseDto? Response
);

/// <summary>
/// Handler for placing a piece on the game board.
/// </summary>
public class PlacePieceCommandHandler : IRequestHandler<PlacePieceCommand, PlacePieceResult>
{
    private readonly IGameSessionRepository _gameSessionRepository;

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

        if (session.Status != GameStatus.Playing)
        {
            return new PlacePieceResult(Found: true, GameActive: false, Response: null);
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
                    IsGameOver: engine.IsGameOver
                )
            );
        }

        // Update session with new state
        var newState = engine.GetState();
        var boardState = SerializeBoardState(newState.BoardGrid);
        var piecesJson = SerializePieces(newState.CurrentPieceTypes);
        
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
                IsGameOver: placementResult.IsGameOver
            )
        );
    }

    private static GameState DeserializeGameState(Core.Entities.GameSession session)
    {
        // Deserialize board state
        var boardDoc = JsonDocument.Parse(session.BoardState);
        var gridElement = boardDoc.RootElement.GetProperty("grid");
        var grid = DeserializeGrid(gridElement);

        // Deserialize pieces
        var piecesArray = JsonSerializer.Deserialize<string[]>(session.CurrentPieces) ?? [];
        var pieceTypes = piecesArray.Select(p => Enum.Parse<PieceType>(p)).ToList();

        return new GameState(
            BoardGrid: grid,
            CurrentPieceTypes: pieceTypes,
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
        var grid = new string?[rows, 10];

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

    private static string SerializePieces(List<PieceType> pieces)
    {
        var pieceStrings = pieces.Select(p => p.ToString()).ToList();
        return JsonSerializer.Serialize(pieceStrings);
    }

    private static GameStateDto MapToDto(Core.Entities.GameSession session, GameEngine engine)
    {
        var state = engine.GetState();
        
        return new GameStateDto(
            Id: session.Id.ToString(),
            Grid: ConvertGrid(state.BoardGrid),
            CurrentPieces: state.CurrentPieceTypes.Select(MapPieceToDto).ToArray(),
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

    private static PieceDto MapPieceToDto(PieceType pieceType)
    {
        var piece = Piece.Create(pieceType);
        var shape = ConvertShapeToArray(piece.Shape);
        
        return new PieceDto(
            Type: MapPieceType(pieceType),
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
        _ => throw new ArgumentOutOfRangeException(nameof(pieceType))
    };
}
