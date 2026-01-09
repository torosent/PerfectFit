namespace PerfectFit.Core.GameLogic.Scoring;

/// <summary>
/// Calculates scores based on lines cleared and combo multipliers.
/// Enhanced scoring system with bigger rewards for skill!
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// Base points awarded per line cleared.
    /// </summary>
    public const int PointsPerLine = 10;

    /// <summary>
    /// Calculates the bonus points for clearing lines.
    /// Enhanced rewards for clearing multiple lines at once!
    /// Uses interesting non-round numbers for more exciting scores.
    /// </summary>
    /// <param name="linesCleared">The number of lines cleared (rows + columns).</param>
    /// <returns>The bonus points before combo multiplier.</returns>
    public static int CalculateLineBonus(int linesCleared)
    {
        if (linesCleared <= 0)
            return 0;

        return linesCleared switch
        {
            1 => 127,           // Single line - good!
            2 => 319,           // Two lines - great!
            3 => 673,           // Three lines - amazing!
            4 => 1249,          // Four lines - incredible!
            5 => 1847,          // Five lines - legendary!
            _ => 1847 + ((linesCleared - 5) * 512) // 6=2359, 7=2871, etc.
        };
    }

    /// <summary>
    /// Gets the combo multiplier based on current combo count.
    /// Enhanced combo system rewards consistent play!
    /// Combo 0 = 1x, 1 = 1.5x, 2 = 2x, 3 = 3x, 4 = 4x, 5+ = 5x+
    /// </summary>
    /// <param name="comboCount">The current combo count.</param>
    /// <returns>The multiplier to apply to points.</returns>
    public static double GetComboMultiplier(int comboCount)
    {
        if (comboCount <= 0)
            return 1.0;

        // More generous combo scaling
        return comboCount switch
        {
            1 => 1.5,
            2 => 2.0,
            3 => 3.0,
            4 => 4.0,
            5 => 5.0,
            _ => 5.0 + ((comboCount - 5) * 0.5) // 6=5.5x, 7=6x, etc.
        };
    }

    /// <summary>
    /// Calculates total points earned for a line clear.
    /// Total = LineBonus * ComboMultiplier
    /// </summary>
    /// <param name="linesCleared">The number of lines cleared.</param>
    /// <param name="currentCombo">The current combo count.</param>
    /// <returns>The total points earned, rounded down to integer.</returns>
    public static int CalculatePoints(int linesCleared, int currentCombo)
    {
        var lineBonus = CalculateLineBonus(linesCleared);
        var multiplier = GetComboMultiplier(currentCombo);
        return (int)(lineBonus * multiplier);
    }
}
