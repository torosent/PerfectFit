namespace PerfectFit.Core.Services.Results;

/// <summary>
/// Statistics calculated from a user's game history.
/// </summary>
/// <param name="AverageScore">The user's average score across all games.</param>
/// <param name="BestScore">The user's highest score.</param>
/// <param name="TotalGames">Total number of games played.</param>
/// <param name="Accuracy">Percentage of successful piece placements (0-100).</param>
/// <param name="AverageLinesCleared">Average lines cleared per game.</param>
/// <param name="TotalLinesCleared">Total lines cleared across all games.</param>
/// <param name="AverageCombo">Average max combo per game.</param>
/// <param name="BestCombo">Highest combo ever achieved.</param>
public record UserStats(
    double AverageScore,
    int BestScore,
    int TotalGames,
    double Accuracy,
    double AverageLinesCleared,
    int TotalLinesCleared,
    double AverageCombo,
    int BestCombo
);
