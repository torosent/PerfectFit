using MediatR;
using PerfectFit.Core.GameLogic;
using PerfectFit.Core.GameLogic.Pieces;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Games.DTOs;
using System.Text.Json;

namespace PerfectFit.UseCases.Games.Queries;

/// <summary>
/// Query to get the current state of a game.
/// </summary>
/// <param name="GameId">The ID of the game to retrieve.</param>
public record GetGameQuery(Guid GameId) : IRequest<GameStateDto?>;

/// <summary>
/// Handler for retrieving game state.
/// </summary>
public class GetGameQueryHandler : IRequestHandler<GetGameQuery, GameStateDto?>
{
    private readonly IGameSessionRepository _gameSessionRepository;

    public GetGameQueryHandler(IGameSessionRepository gameSessionRepository)
    {
        _gameSessionRepository = gameSessionRepository;
    }

    public async Task<GameStateDto?> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        var session = await _gameSessionRepository.GetByIdAsync(request.GameId, cancellationToken);

        if (session is null)
        {
            return null;
        }

        // Reconstruct game engine from saved state
        var gameState = DeserializeGameState(session);
        var engine = GameEngine.FromState(gameState);

        return MapToDto(session, engine);
    }

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

    private record StoredPiece(PieceType Type, int Rotation);

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
            Status: session.Status == Core.Enums.GameStatus.Playing
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
