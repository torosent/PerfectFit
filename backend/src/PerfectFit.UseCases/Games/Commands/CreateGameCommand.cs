using MediatR;
using PerfectFit.Core.Entities;
using PerfectFit.Core.GameLogic;
using PerfectFit.Core.GameLogic.Pieces;
using PerfectFit.Core.Interfaces;
using PerfectFit.UseCases.Games.DTOs;
using System.Text.Json;

namespace PerfectFit.UseCases.Games.Commands;

/// <summary>
/// Command to create a new game session.
/// </summary>
/// <param name="UserId">Optional user ID for authenticated users.</param>
public record CreateGameCommand(int? UserId) : IRequest<GameStateDto>;

/// <summary>
/// Handler for creating a new game session.
/// </summary>
public class CreateGameCommandHandler : IRequestHandler<CreateGameCommand, GameStateDto>
{
    private readonly IGameSessionRepository _gameSessionRepository;

    public CreateGameCommandHandler(IGameSessionRepository gameSessionRepository)
    {
        _gameSessionRepository = gameSessionRepository;
    }

    public async Task<GameStateDto> Handle(CreateGameCommand request, CancellationToken cancellationToken)
    {
        // Create new game engine
        var engine = new GameEngine();
        var state = engine.GetState();

        // Create game session entity
        var session = GameSession.Create(request.UserId);

        // Serialize and update game state
        var boardState = SerializeBoardState(state.BoardGrid);
        var piecesJson = SerializePieces(state.CurrentPieces);
        session.UpdateBoard(boardState, piecesJson, state.PieceBagState);

        // Save to database
        await _gameSessionRepository.AddAsync(session, cancellationToken);

        // Return DTO
        return MapToDto(session, engine);
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

    private static GameStateDto MapToDto(GameSession session, GameEngine engine)
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
