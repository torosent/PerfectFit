namespace PerfectFit.UseCases.Games.DTOs;

/// <summary>
/// Piece types available in the game
/// </summary>
public enum PieceTypeDto
{
    // Tetrominoes
    I, O, T, S, Z, J, L,
    // Lines
    DOT, LINE2, LINE3, LINE5,
    // Corners
    CORNER, BIG_CORNER,
    // Squares
    SQUARE_2X2, SQUARE_3X3,
    // Rectangles
    RECT_2X3, RECT_3X2
}

/// <summary>
/// Game status
/// </summary>
public enum GameStatusDto
{
    Playing,
    Ended
}

/// <summary>
/// Represents a position on the game board
/// </summary>
public record PositionDto(int Row, int Col);

/// <summary>
/// Represents a game piece
/// </summary>
public record PieceDto(
    PieceTypeDto Type,
    int[][] Shape,
    string Color
);

/// <summary>
/// Represents the current state of a game
/// </summary>
public record GameStateDto(
    string Id,
    string?[][] Grid,
    PieceDto[] CurrentPieces,
    int Score,
    int Combo,
    GameStatusDto Status,
    int LinesCleared
);

/// <summary>
/// Request to place a piece on the board
/// </summary>
public record PlacePieceRequestDto(
    int PieceIndex,
    PositionDto Position,
    long? ClientTimestamp = null
);

/// <summary>
/// Response after placing a piece
/// </summary>
public record PlacePieceResponseDto(
    bool Success,
    GameStateDto GameState,
    int LinesCleared,
    int PointsEarned,
    bool IsGameOver,
    int PiecesRemainingInTurn,
    bool NewTurnStarted
);
