using FluentAssertions;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.UnitTests.GameLogic;

/// <summary>
/// Tests for piece definitions ensuring all shapes have correct cell counts and colors.
/// </summary>
public class PieceDefinitionsTests
{
    [Theory]
    [InlineData(PieceType.I, 4)]
    [InlineData(PieceType.O, 4)]
    [InlineData(PieceType.T, 4)]
    [InlineData(PieceType.S, 4)]
    [InlineData(PieceType.Z, 4)]
    [InlineData(PieceType.J, 4)]
    [InlineData(PieceType.L, 4)]
    [InlineData(PieceType.Dot, 1)]
    [InlineData(PieceType.Line2, 2)]
    [InlineData(PieceType.Line3, 3)]
    [InlineData(PieceType.Line5, 5)]
    [InlineData(PieceType.Corner, 3)]
    [InlineData(PieceType.BigCorner, 5)]
    [InlineData(PieceType.Square2x2, 4)]
    [InlineData(PieceType.Square3x3, 9)]
    public void GetShape_Should_Return_Correct_Cell_Count(PieceType type, int expectedCells)
    {
        // Act
        var shape = PieceDefinitions.GetShape(type);
        var cellCount = CountCells(shape);

        // Assert
        cellCount.Should().Be(expectedCells);
    }

    [Theory]
    [InlineData(PieceType.I, 1, 4)]  // 1 row, 4 columns
    [InlineData(PieceType.O, 2, 2)]  // 2 rows, 2 columns
    [InlineData(PieceType.T, 2, 3)]  // 2 rows, 3 columns
    [InlineData(PieceType.S, 2, 3)]
    [InlineData(PieceType.Z, 2, 3)]
    [InlineData(PieceType.J, 3, 2)]  // 3 rows, 2 columns
    [InlineData(PieceType.L, 3, 2)]
    [InlineData(PieceType.Dot, 1, 1)]
    [InlineData(PieceType.Line2, 1, 2)]
    [InlineData(PieceType.Line3, 1, 3)]
    [InlineData(PieceType.Line5, 1, 5)]
    [InlineData(PieceType.Corner, 2, 2)]
    [InlineData(PieceType.BigCorner, 3, 3)]
    [InlineData(PieceType.Square2x2, 2, 2)]
    [InlineData(PieceType.Square3x3, 3, 3)]
    public void GetShape_Should_Return_Correct_Dimensions(PieceType type, int expectedRows, int expectedCols)
    {
        // Act
        var shape = PieceDefinitions.GetShape(type);

        // Assert
        shape.GetLength(0).Should().Be(expectedRows);
        shape.GetLength(1).Should().Be(expectedCols);
    }

    [Theory]
    [InlineData(PieceType.I, "#00FFFF")]
    [InlineData(PieceType.O, "#FFFF00")]
    [InlineData(PieceType.T, "#800080")]
    [InlineData(PieceType.S, "#00FF00")]
    [InlineData(PieceType.Z, "#FF0000")]
    [InlineData(PieceType.J, "#0000FF")]
    [InlineData(PieceType.L, "#FFA500")]
    [InlineData(PieceType.Dot, "#808080")]
    [InlineData(PieceType.Line2, "#FFB6C1")]
    [InlineData(PieceType.Line3, "#90EE90")]
    [InlineData(PieceType.Line5, "#87CEEB")]
    [InlineData(PieceType.Corner, "#DDA0DD")]
    [InlineData(PieceType.BigCorner, "#F0E68C")]
    [InlineData(PieceType.Square2x2, "#CD853F")]
    [InlineData(PieceType.Square3x3, "#8B4513")]
    public void GetColor_Should_Return_Correct_Hex_Color(PieceType type, string expectedColor)
    {
        // Act
        var color = PieceDefinitions.GetColor(type);

        // Assert
        color.Should().Be(expectedColor);
    }

    [Fact]
    public void AllPieceTypes_Should_Have_Definitions()
    {
        // Arrange
        var allTypes = Enum.GetValues<PieceType>();

        // Act & Assert
        foreach (var type in allTypes)
        {
            var shape = PieceDefinitions.GetShape(type);
            var color = PieceDefinitions.GetColor(type);

            shape.Should().NotBeNull();
            color.Should().NotBeNullOrEmpty();
            color.Should().StartWith("#");
        }
    }

    [Fact]
    public void Piece_Create_Should_Return_Piece_With_Correct_Properties()
    {
        // Act
        var piece = Piece.Create(PieceType.T);

        // Assert
        piece.Type.Should().Be(PieceType.T);
        piece.Color.Should().Be("#800080");
        piece.Shape.GetLength(0).Should().Be(2);
        piece.Shape.GetLength(1).Should().Be(3);
    }

    [Fact]
    public void I_Piece_Should_Be_Horizontal_Line()
    {
        // Act
        var shape = PieceDefinitions.GetShape(PieceType.I);

        // Assert - {{true,true,true,true}} (1x4 horizontal)
        shape[0, 0].Should().BeTrue();
        shape[0, 1].Should().BeTrue();
        shape[0, 2].Should().BeTrue();
        shape[0, 3].Should().BeTrue();
    }

    [Fact]
    public void T_Piece_Should_Have_Correct_Shape()
    {
        // Act
        var shape = PieceDefinitions.GetShape(PieceType.T);

        // Assert - {{true,true,true},{false,true,false}}
        shape[0, 0].Should().BeTrue();
        shape[0, 1].Should().BeTrue();
        shape[0, 2].Should().BeTrue();
        shape[1, 0].Should().BeFalse();
        shape[1, 1].Should().BeTrue();
        shape[1, 2].Should().BeFalse();
    }

    [Fact]
    public void Corner_Piece_Should_Have_L_Shape()
    {
        // Act
        var shape = PieceDefinitions.GetShape(PieceType.Corner);

        // Assert - {{true,true},{true,false}} (2x2, 3 cells)
        shape[0, 0].Should().BeTrue();
        shape[0, 1].Should().BeTrue();
        shape[1, 0].Should().BeTrue();
        shape[1, 1].Should().BeFalse();
    }

    [Fact]
    public void BigCorner_Piece_Should_Have_Correct_Shape()
    {
        // Act
        var shape = PieceDefinitions.GetShape(PieceType.BigCorner);

        // Assert - {{true,true,true},{true,false,false},{true,false,false}}
        shape[0, 0].Should().BeTrue();
        shape[0, 1].Should().BeTrue();
        shape[0, 2].Should().BeTrue();
        shape[1, 0].Should().BeTrue();
        shape[1, 1].Should().BeFalse();
        shape[1, 2].Should().BeFalse();
        shape[2, 0].Should().BeTrue();
        shape[2, 1].Should().BeFalse();
        shape[2, 2].Should().BeFalse();
    }

    private static int CountCells(bool[,] shape)
    {
        int count = 0;
        for (int row = 0; row < shape.GetLength(0); row++)
        {
            for (int col = 0; col < shape.GetLength(1); col++)
            {
                if (shape[row, col])
                    count++;
            }
        }
        return count;
    }
}
