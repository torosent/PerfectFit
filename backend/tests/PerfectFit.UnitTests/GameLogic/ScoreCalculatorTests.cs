using FluentAssertions;
using PerfectFit.Core.GameLogic.Scoring;

namespace PerfectFit.UnitTests.GameLogic;

public class ScoreCalculatorTests
{
    [Fact]
    public void PointsPerLine_ShouldBeTen()
    {
        ScoreCalculator.PointsPerLine.Should().Be(10);
    }

    #region CalculateLineBonus Tests

    [Fact]
    public void CalculateLineBonus_ZeroLines_ReturnsZero()
    {
        var result = ScoreCalculator.CalculateLineBonus(0);
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateLineBonus_OneLine_Returns127()
    {
        var result = ScoreCalculator.CalculateLineBonus(1);
        result.Should().Be(127);
    }

    [Fact]
    public void CalculateLineBonus_TwoLines_Returns319()
    {
        var result = ScoreCalculator.CalculateLineBonus(2);
        result.Should().Be(319);
    }

    [Fact]
    public void CalculateLineBonus_ThreeLines_Returns673()
    {
        var result = ScoreCalculator.CalculateLineBonus(3);
        result.Should().Be(673);
    }

    [Fact]
    public void CalculateLineBonus_FourLines_Returns1249()
    {
        var result = ScoreCalculator.CalculateLineBonus(4);
        result.Should().Be(1249);
    }

    [Fact]
    public void CalculateLineBonus_FiveLines_Returns1847()
    {
        var result = ScoreCalculator.CalculateLineBonus(5);
        result.Should().Be(1847);
    }

    [Fact]
    public void CalculateLineBonus_SixOrMoreLines_ScalesUp()
    {
        // 6 lines = 1847 + 512 = 2359
        var result = ScoreCalculator.CalculateLineBonus(6);
        result.Should().Be(2359);
        
        // 7 lines = 1847 + 1024 = 2871
        var result7 = ScoreCalculator.CalculateLineBonus(7);
        result7.Should().Be(2871);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void CalculateLineBonus_NegativeLines_ReturnsZero(int lines)
    {
        var result = ScoreCalculator.CalculateLineBonus(lines);
        result.Should().Be(0);
    }

    #endregion

    #region GetComboMultiplier Tests

    [Fact]
    public void GetComboMultiplier_ComboZero_ReturnsOne()
    {
        var result = ScoreCalculator.GetComboMultiplier(0);
        result.Should().Be(1.0);
    }

    [Fact]
    public void GetComboMultiplier_ComboOne_ReturnsOnePointFive()
    {
        var result = ScoreCalculator.GetComboMultiplier(1);
        result.Should().Be(1.5);
    }

    [Fact]
    public void GetComboMultiplier_ComboTwo_ReturnsTwo()
    {
        var result = ScoreCalculator.GetComboMultiplier(2);
        result.Should().Be(2.0);
    }

    [Fact]
    public void GetComboMultiplier_ComboThree_ReturnsThree()
    {
        var result = ScoreCalculator.GetComboMultiplier(3);
        result.Should().Be(3.0);
    }

    [Fact]
    public void GetComboMultiplier_ComboFour_ReturnsFour()
    {
        var result = ScoreCalculator.GetComboMultiplier(4);
        result.Should().Be(4.0);
    }

    [Fact]
    public void GetComboMultiplier_ComboFive_ReturnsFive()
    {
        var result = ScoreCalculator.GetComboMultiplier(5);
        result.Should().Be(5.0);
    }

    [Fact]
    public void GetComboMultiplier_ComboTen_ReturnsSevenPointFive()
    {
        // Pattern: 5.0 + ((combo - 5) * 0.5) = 5.0 + (5 * 0.5) = 7.5
        var result = ScoreCalculator.GetComboMultiplier(10);
        result.Should().Be(7.5);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-5)]
    public void GetComboMultiplier_NegativeCombo_ReturnsOne(int combo)
    {
        var result = ScoreCalculator.GetComboMultiplier(combo);
        result.Should().Be(1.0);
    }

    #endregion

    #region CalculatePoints Tests

    [Fact]
    public void CalculatePoints_OneLineNoCombo_Returns127()
    {
        // 1 line = 127 points, combo 0 = 1x multiplier
        var result = ScoreCalculator.CalculatePoints(1, 0);
        result.Should().Be(127);
    }

    [Fact]
    public void CalculatePoints_TwoLinesNoCombo_Returns319()
    {
        // 2 lines = 319 points, combo 0 = 1x multiplier
        var result = ScoreCalculator.CalculatePoints(2, 0);
        result.Should().Be(319);
    }

    [Fact]
    public void CalculatePoints_OneLineComboOne_Returns190()
    {
        // 1 line = 127 points, combo 1 = 1.5x multiplier = 190.5 -> 190
        var result = ScoreCalculator.CalculatePoints(1, 1);
        result.Should().Be(190);
    }

    [Fact]
    public void CalculatePoints_TwoLinesComboTwo_Returns638()
    {
        // 2 lines = 319 points, combo 2 = 2x multiplier = 638
        var result = ScoreCalculator.CalculatePoints(2, 2);
        result.Should().Be(638);
    }

    [Fact]
    public void CalculatePoints_FourLinesComboThree_Returns3747()
    {
        // 4 lines = 1249 points, combo 3 = 3x multiplier = 3747
        var result = ScoreCalculator.CalculatePoints(4, 3);
        result.Should().Be(3747);
    }

    [Fact]
    public void CalculatePoints_FiveLinesComboFive_Returns9235()
    {
        // 5 lines = 1847 points, combo 5 = 5x multiplier = 9235
        var result = ScoreCalculator.CalculatePoints(5, 5);
        result.Should().Be(9235);
    }

    [Fact]
    public void CalculatePoints_ZeroLines_ReturnsZero()
    {
        var result = ScoreCalculator.CalculatePoints(0, 5);
        result.Should().Be(0);
    }

    [Fact]
    public void CalculatePoints_RoundsDown()
    {
        // Test that fractional points are rounded down (integer result)
        // 1 line = 100 points, combo 1 = 1.5x = 150 (no rounding needed)
        // But let's verify the result is always an integer
        var result = ScoreCalculator.CalculatePoints(1, 1);
        result.Should().BeOfType(typeof(int));
    }

    #endregion
}
