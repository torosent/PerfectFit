namespace PerfectFit.Core.GameLogic.Scoring;

/// <summary>
/// Calculates scores based on lines cleared and combo multipliers.
/// </summary>
public static class ScoreCalculator
{
    /// <summary>
    /// Base points awarded per line cleared.
    /// </summary>
    public const int PointsPerLine = 10;

    /// <summary>
    /// Calculates the bonus points for clearing lines.
    /// 1 line = 10, 2 lines = 30, 3 lines = 60, 4 lines = 100, 5+ lines = 150+
    /// </summary>
    /// <param name="linesCleared">The number of lines cleared.</param>
    /// <returns>The bonus points before combo multiplier.</returns>
    public static int CalculateLineBonus(int linesCleared)
    {
        if (linesCleared <= 0)
            return 0;

        return linesCleared switch
        {
            1 => 10,
            2 => 30,
            3 => 60,
            4 => 100,
            _ => 150 + ((linesCleared - 5) * 50) // 5=150, 6=200, etc.
        };
    }

    /// <summary>
    /// Gets the combo multiplier based on current combo count.
    /// Combo 0 = 1x, 1 = 1.5x, 2 = 2x, 3 = 2.5x, etc.
    /// </summary>
    /// <param name="comboCount">The current combo count.</param>
    /// <returns>The multiplier to apply to points.</returns>
    public static double GetComboMultiplier(int comboCount)
    {
        if (comboCount < 0)
            return 1.0;

        return 1.0 + (comboCount * 0.5);
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
