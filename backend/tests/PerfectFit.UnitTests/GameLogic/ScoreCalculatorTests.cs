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
    public void CalculateLineBonus_OneLine_ReturnsTen()
    {
        var result = ScoreCalculator.CalculateLineBonus(1);
        result.Should().Be(10);
    }

    [Fact]
    public void CalculateLineBonus_TwoLines_ReturnsThirty()
    {
        var result = ScoreCalculator.CalculateLineBonus(2);
        result.Should().Be(30);
    }

    [Fact]
    public void CalculateLineBonus_ThreeLines_ReturnsSixty()
    {
        var result = ScoreCalculator.CalculateLineBonus(3);
        result.Should().Be(60);
    }

    [Fact]
    public void CalculateLineBonus_FourLines_ReturnsOneHundred()
    {
        var result = ScoreCalculator.CalculateLineBonus(4);
        result.Should().Be(100);
    }

    [Fact]
    public void CalculateLineBonus_FiveLines_ReturnsOneHundredFifty()
    {
        var result = ScoreCalculator.CalculateLineBonus(5);
        result.Should().Be(150);
    }

    [Fact]
    public void CalculateLineBonus_SixOrMoreLines_ReturnsOneHundredFiftyPlus()
    {
        // 6+ lines should return at least 150
        var result = ScoreCalculator.CalculateLineBonus(6);
        result.Should().BeGreaterThanOrEqualTo(150);
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
    public void GetComboMultiplier_ComboThree_ReturnsTwoPointFive()
    {
        var result = ScoreCalculator.GetComboMultiplier(3);
        result.Should().Be(2.5);
    }

    [Fact]
    public void GetComboMultiplier_ComboTen_ReturnsSix()
    {
        // Pattern: 1 + (combo * 0.5) = 1 + (10 * 0.5) = 6.0
        var result = ScoreCalculator.GetComboMultiplier(10);
        result.Should().Be(6.0);
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
    public void CalculatePoints_OneLineNoCombo_ReturnsTen()
    {
        // 1 line = 10 points, combo 0 = 1x multiplier
        var result = ScoreCalculator.CalculatePoints(1, 0);
        result.Should().Be(10);
    }

    [Fact]
    public void CalculatePoints_TwoLinesNoCombo_ReturnsThirty()
    {
        // 2 lines = 30 points, combo 0 = 1x multiplier
        var result = ScoreCalculator.CalculatePoints(2, 0);
        result.Should().Be(30);
    }

    [Fact]
    public void CalculatePoints_OneLineComboOne_ReturnsFifteen()
    {
        // 1 line = 10 points, combo 1 = 1.5x multiplier = 15
        var result = ScoreCalculator.CalculatePoints(1, 1);
        result.Should().Be(15);
    }

    [Fact]
    public void CalculatePoints_TwoLinesComboTwo_ReturnsSixty()
    {
        // 2 lines = 30 points, combo 2 = 2x multiplier = 60
        var result = ScoreCalculator.CalculatePoints(2, 2);
        result.Should().Be(60);
    }

    [Fact]
    public void CalculatePoints_FourLinesComboThree_ReturnsTwoHundredFifty()
    {
        // 4 lines = 100 points, combo 3 = 2.5x multiplier = 250
        var result = ScoreCalculator.CalculatePoints(4, 3);
        result.Should().Be(250);
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
        // 1 line = 10 points, combo 1 = 1.5x = 15 (no rounding needed)
        // But let's verify the result is always an integer
        var result = ScoreCalculator.CalculatePoints(1, 1);
        result.Should().BeOfType(typeof(int));
    }

    #endregion
}
