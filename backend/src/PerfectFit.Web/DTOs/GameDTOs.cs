namespace PerfectFit.Web.DTOs;

/// <summary>
/// Piece types available in the game
/// </summary>
public enum PieceTypeWebDto
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
    RECT_2X3
}

/// <summary>
/// Game status
/// </summary>
public enum GameStatusWebDto
{
    Playing,
    Ended
}

/// <summary>
/// Represents a position on the game board
/// </summary>
public record PositionWebDto(int Row, int Col);

/// <summary>
/// Represents a game piece
/// </summary>
public record PieceWebDto(
    PieceTypeWebDto Type,
    int[][] Shape,
    string Color
);

/// <summary>
/// Represents the current state of a game
/// </summary>
public record GameStateWebDto(
    string Id,
    string?[][] Grid,
    PieceWebDto[] CurrentPieces,
    int Score,
    int Combo,
    GameStatusWebDto Status,
    int LinesCleared
);

/// <summary>
/// Response after ending a game (web layer DTO)
/// </summary>
public record GameEndResponseWebDto(
    GameStateWebDto GameState,
    GameEndGamificationDto? Gamification
);

/// <summary>
/// Authentication provider
/// </summary>
public enum AuthProviderDto
{
    Google,
    Facebook,
    Microsoft,
    Guest
}

/// <summary>
/// Request to place a piece on the board
/// </summary>
public record PlacePieceRequestDto(
    int PieceIndex,
    PositionWebDto Position,
    long? ClientTimestamp = null
);

/// <summary>
/// Response after placing a piece
/// </summary>
public record PlacePieceResponseDto(
    bool Success,
    GameStateWebDto GameState,
    int LinesCleared,
    int PointsEarned,
    bool IsGameOver
);

/// <summary>
/// Represents a user's profile
/// </summary>
public record UserProfileDto(
    string Id,
    string DisplayName,
    string? Email,
    AuthProviderDto Provider,
    int HighScore,
    int GamesPlayed
);
