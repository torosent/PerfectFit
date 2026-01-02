using FluentAssertions;
using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.UnitTests.GameLogic;

/// <summary>
/// Tests for GameBoard functionality including placement, bounds checking, and validation.
/// </summary>
public class GameBoardTests
{
    [Fact]
    public void NewBoard_Should_Be_10x10_And_Empty()
    {
        // Act
        var board = new GameBoard();

        // Assert
        var array = board.ToArray();
        array.GetLength(0).Should().Be(10);
        array.GetLength(1).Should().Be(10);

        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                array[row, col].Should().BeNull();
            }
        }
    }

    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(9, 9, true)]
    [InlineData(5, 5, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, -1, false)]
    [InlineData(10, 0, false)]
    [InlineData(0, 10, false)]
    [InlineData(10, 10, false)]
    public void IsInBounds_Should_Return_Correct_Result(int row, int col, bool expected)
    {
        // Arrange
        var board = new GameBoard();

        // Act
        var result = board.IsInBounds(row, col);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCell_Should_Return_Null_For_Empty_Cell()
    {
        // Arrange
        var board = new GameBoard();

        // Act
        var cell = board.GetCell(5, 5);

        // Assert
        cell.Should().BeNull();
    }

    [Fact]
    public void IsEmpty_Should_Return_True_For_Empty_Cell()
    {
        // Arrange
        var board = new GameBoard();

        // Act
        var result = board.IsEmpty(5, 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanPlacePiece_Should_Return_True_For_Valid_Placement()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.Dot);

        // Act
        var result = board.CanPlacePiece(piece, 0, 0);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanPlacePiece_Should_Return_False_When_Out_Of_Bounds()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.I); // 1x4 horizontal

        // Act - try to place at column 8 (would extend to column 11)
        var result = board.CanPlacePiece(piece, 0, 8);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanPlacePiece_Should_Return_False_When_Overlapping()
    {
        // Arrange
        var board = new GameBoard();
        var piece1 = Piece.Create(PieceType.Dot);
        var piece2 = Piece.Create(PieceType.Dot);
        board.TryPlacePiece(piece1, 5, 5);

        // Act
        var result = board.CanPlacePiece(piece2, 5, 5);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryPlacePiece_Should_Return_True_And_Place_Piece()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.Dot);

        // Act
        var result = board.TryPlacePiece(piece, 5, 5);

        // Assert
        result.Should().BeTrue();
        board.GetCell(5, 5).Should().Be(piece.Color);
    }

    [Fact]
    public void TryPlacePiece_Should_Return_False_When_Cannot_Place()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.I);

        // Act - try to place out of bounds
        var result = board.TryPlacePiece(piece, 0, 8);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void TryPlacePiece_Should_Place_Multi_Cell_Piece_Correctly()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.I); // 1x4 horizontal

        // Act
        var result = board.TryPlacePiece(piece, 0, 0);

        // Assert
        result.Should().BeTrue();
        board.GetCell(0, 0).Should().Be(piece.Color);
        board.GetCell(0, 1).Should().Be(piece.Color);
        board.GetCell(0, 2).Should().Be(piece.Color);
        board.GetCell(0, 3).Should().Be(piece.Color);
        board.GetCell(0, 4).Should().BeNull(); // Next cell should be empty
    }

    [Fact]
    public void TryPlacePiece_Should_Handle_Complex_Shape()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.T); // {{true,true,true},{false,true,false}}

        // Act
        var result = board.TryPlacePiece(piece, 0, 0);

        // Assert
        result.Should().BeTrue();
        board.GetCell(0, 0).Should().Be(piece.Color);
        board.GetCell(0, 1).Should().Be(piece.Color);
        board.GetCell(0, 2).Should().Be(piece.Color);
        board.GetCell(1, 0).Should().BeNull(); // false in shape
        board.GetCell(1, 1).Should().Be(piece.Color);
        board.GetCell(1, 2).Should().BeNull(); // false in shape
    }

    [Fact]
    public void CanPlacePieceAnywhere_Should_Return_True_For_Empty_Board()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.Square3x3);

        // Act
        var result = board.CanPlacePieceAnywhere(piece);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanPlacePieceAnywhere_Should_Return_False_When_Board_Too_Full()
    {
        // Arrange
        var board = new GameBoard();
        // Fill the board with dots leaving no 3x3 space
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                // Leave checkerboard pattern
                if ((row + col) % 2 == 0)
                {
                    board.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
                }
            }
        }
        var piece = Piece.Create(PieceType.Square2x2); // 2x2 block can't fit on checkerboard

        // Act
        var result = board.CanPlacePieceAnywhere(piece);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetValidPositions_Should_Return_All_Valid_Positions()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.Dot);

        // Act
        var positions = board.GetValidPositions(piece);

        // Assert - empty 10x10 board should have 100 valid positions for 1x1 piece
        positions.Should().HaveCount(100);
    }

    [Fact]
    public void GetValidPositions_Should_Exclude_Invalid_Positions()
    {
        // Arrange
        var board = new GameBoard();
        board.TryPlacePiece(Piece.Create(PieceType.Dot), 5, 5);
        var piece = Piece.Create(PieceType.Dot);

        // Act
        var positions = board.GetValidPositions(piece);

        // Assert - 99 positions (one is occupied)
        positions.Should().HaveCount(99);
        positions.Should().NotContain((5, 5));
    }

    [Fact]
    public void GetValidPositions_Should_Account_For_Piece_Size()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.I); // 1x4 horizontal

        // Act
        var positions = board.GetValidPositions(piece);

        // Assert - can place at columns 0-6 (7 positions) for each of 10 rows = 70
        positions.Should().HaveCount(70);
    }

    [Fact]
    public void ToArray_Should_Return_Copy_Of_Board_State()
    {
        // Arrange
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.Dot);
        board.TryPlacePiece(piece, 0, 0);

        // Act
        var array = board.ToArray();
        array[0, 0] = null; // Modify the copy

        // Assert - original board should be unchanged
        board.GetCell(0, 0).Should().Be(piece.Color);
    }

    [Fact]
    public void FromArray_Should_Create_Board_From_Array()
    {
        // Arrange
        var source = new string?[10, 10];
        source[0, 0] = "#FF0000";
        source[5, 5] = "#00FF00";

        // Act
        var board = GameBoard.FromArray(source);

        // Assert
        board.GetCell(0, 0).Should().Be("#FF0000");
        board.GetCell(5, 5).Should().Be("#00FF00");
        board.GetCell(9, 9).Should().BeNull();
    }

    [Fact]
    public void FromArray_Should_Throw_For_Invalid_Dimensions()
    {
        // Arrange
        var source = new string?[5, 5];

        // Act & Assert
        var act = () => GameBoard.FromArray(source);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsEmpty_Should_Return_False_For_Occupied_Cell()
    {
        // Arrange
        var board = new GameBoard();
        board.TryPlacePiece(Piece.Create(PieceType.Dot), 3, 3);

        // Act
        var result = board.IsEmpty(3, 3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmpty_Should_Return_False_For_Out_Of_Bounds()
    {
        // Arrange
        var board = new GameBoard();

        // Act
        var result = board.IsEmpty(-1, 0);

        // Assert
        result.Should().BeFalse();
    }
}
