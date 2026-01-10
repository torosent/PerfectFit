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
    RECT_2X3
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

/// <summary>
/// Response after ending a game
/// </summary>
public record GameEndResponseDto(
    GameStateDto GameState,
    GameEndGamificationResponseDto? Gamification
);

/// <summary>
/// Gamification updates returned after a game ends.
/// </summary>
public record GameEndGamificationResponseDto(
    StreakResponseDto Streak,
    IReadOnlyList<ChallengeProgressResponseDto> ChallengeUpdates,
    IReadOnlyList<AchievementUnlockResponseDto> NewAchievements,
    SeasonXPResponseDto SeasonProgress,
    IReadOnlyList<GoalProgressResponseDto> GoalUpdates,
    int GamesPlayed,
    int HighScore
);

/// <summary>
/// Streak update in game end response.
/// </summary>
public record StreakResponseDto(
    int CurrentStreak,
    int LongestStreak,
    int? FreezeTokens,
    bool? IsAtRisk,
    DateTimeOffset? ResetTime
);

/// <summary>
/// Challenge progress update.
/// </summary>
public record ChallengeProgressResponseDto(
    int ChallengeId,
    string ChallengeName,
    int NewProgress,
    bool JustCompleted,
    int? XPEarned
);

/// <summary>
/// Achievement unlock notification.
/// </summary>
public record AchievementUnlockResponseDto(
    int AchievementId,
    string Name,
    string Description,
    string IconUrl,
    string RewardType,
    int RewardValue
);

/// <summary>
/// Season XP progress update.
/// </summary>
public record SeasonXPResponseDto(
    int XPEarned,
    int TotalXP,
    int NewTier,
    bool TierUp,
    int NewRewardsCount
);

/// <summary>
/// Personal goal progress update.
/// </summary>
public record GoalProgressResponseDto(
    int GoalId,
    string Description,
    int NewProgress,
    bool JustCompleted
);
