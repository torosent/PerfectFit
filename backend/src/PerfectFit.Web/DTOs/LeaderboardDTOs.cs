namespace PerfectFit.Web.DTOs;

/// <summary>
/// Represents a leaderboard entry for API responses.
/// </summary>
public record LeaderboardEntryDto(
    int Rank,
    string DisplayName,
    string? Avatar,
    int Score,
    int LinesCleared,
    int MaxCombo,
    DateTime AchievedAt
);

/// <summary>
/// Request to submit a score to the leaderboard.
/// </summary>
public record SubmitScoreRequestDto(Guid GameSessionId);

/// <summary>
/// Response after submitting a score.
/// </summary>
public record SubmitScoreResponseDto(
    bool Success,
    string? ErrorMessage,
    LeaderboardEntryDto? Entry,
    bool IsNewHighScore,
    int? NewRank
);

/// <summary>
/// User statistics for the leaderboard.
/// </summary>
public record UserStatsDto(
    int HighScore,
    int GamesPlayed,
    int? GlobalRank,
    LeaderboardEntryDto? BestGame
);
