using FluentAssertions;
using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.UnitTests.GameLogic;

/// <summary>
/// Tests for LineClearer functionality including row and column clearing.
/// </summary>
public class LineClearerTests
{
    [Fact]
    public void ClearLines_Should_Return_Empty_Result_For_Empty_Board()
    {
        // Arrange
        var board = new GameBoard();

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().BeEmpty();
        result.ColumnsCleared.Should().BeEmpty();
        result.TotalLinesCleared.Should().Be(0);
    }

    [Fact]
    public void ClearLines_Should_Clear_Complete_Row()
    {
        // Arrange
        var board = new GameBoard();
        // Fill row 0 completely
        for (int col = 0; col < 10; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 0, col);
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().ContainSingle().Which.Should().Be(0);
        result.ColumnsCleared.Should().BeEmpty();
        result.TotalLinesCleared.Should().Be(1);

        // Verify row is cleared
        for (int col = 0; col < 10; col++)
        {
            board.GetCell(0, col).Should().BeNull();
        }
    }

    [Fact]
    public void ClearLines_Should_Clear_Complete_Column()
    {
        // Arrange
        var board = new GameBoard();
        // Fill column 0 completely
        for (int row = 0; row < 10; row++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), row, 0);
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().BeEmpty();
        result.ColumnsCleared.Should().ContainSingle().Which.Should().Be(0);
        result.TotalLinesCleared.Should().Be(1);

        // Verify column is cleared
        for (int row = 0; row < 10; row++)
        {
            board.GetCell(row, 0).Should().BeNull();
        }
    }

    [Fact]
    public void ClearLines_Should_Clear_Multiple_Rows()
    {
        // Arrange
        var board = new GameBoard();
        // Fill rows 0 and 5 completely
        for (int col = 0; col < 10; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 0, col);
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 5, col);
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().HaveCount(2);
        result.RowsCleared.Should().Contain(0);
        result.RowsCleared.Should().Contain(5);
        result.TotalLinesCleared.Should().Be(2);
    }

    [Fact]
    public void ClearLines_Should_Clear_Multiple_Columns()
    {
        // Arrange
        var board = new GameBoard();
        // Fill columns 2 and 7 completely
        for (int row = 0; row < 10; row++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), row, 2);
            board.TryPlacePiece(Piece.Create(PieceType.Dot), row, 7);
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.ColumnsCleared.Should().HaveCount(2);
        result.ColumnsCleared.Should().Contain(2);
        result.ColumnsCleared.Should().Contain(7);
        result.TotalLinesCleared.Should().Be(2);
    }

    [Fact]
    public void ClearLines_Should_Clear_Row_And_Column_Simultaneously()
    {
        // Arrange
        var board = new GameBoard();
        // Fill row 3 completely
        for (int col = 0; col < 10; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 3, col);
        }
        // Fill column 5 completely (row 3 col 5 already filled)
        for (int row = 0; row < 10; row++)
        {
            if (row != 3) // Skip already placed
            {
                board.TryPlacePiece(Piece.Create(PieceType.Dot), row, 5);
            }
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().ContainSingle().Which.Should().Be(3);
        result.ColumnsCleared.Should().ContainSingle().Which.Should().Be(5);
        result.TotalLinesCleared.Should().Be(2);
    }

    [Fact]
    public void ClearLines_Should_Not_Apply_Gravity()
    {
        // Arrange
        var board = new GameBoard();
        // Place a piece above a complete row
        board.TryPlacePiece(Piece.Create(PieceType.Dot), 1, 5);
        // Fill row 2 completely
        for (int col = 0; col < 10; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 2, col);
        }

        // Act
        LineClearer.ClearLines(board);

        // Assert - piece at row 1 should NOT move to row 2
        board.GetCell(1, 5).Should().NotBeNull();
        board.GetCell(2, 5).Should().BeNull(); // Row was cleared
    }

    [Fact]
    public void ClearLines_Should_Not_Clear_Incomplete_Row()
    {
        // Arrange
        var board = new GameBoard();
        // Fill row 0 with 9 cells (missing one)
        for (int col = 0; col < 9; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 0, col);
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().BeEmpty();
        result.TotalLinesCleared.Should().Be(0);

        // Verify row is NOT cleared
        board.GetCell(0, 0).Should().NotBeNull();
    }

    [Fact]
    public void ClearLines_Should_Not_Clear_Incomplete_Column()
    {
        // Arrange
        var board = new GameBoard();
        // Fill column 0 with 9 cells (missing one)
        for (int row = 0; row < 9; row++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), row, 0);
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.ColumnsCleared.Should().BeEmpty();
        result.TotalLinesCleared.Should().Be(0);

        // Verify column is NOT cleared
        board.GetCell(0, 0).Should().NotBeNull();
    }

    [Fact]
    public void ClearResult_TotalLinesCleared_Should_Sum_Rows_And_Columns()
    {
        // Arrange
        var result = new ClearResult(
            RowsCleared: new List<int> { 0, 5 },
            ColumnsCleared: new List<int> { 2, 7, 9 }
        );

        // Act & Assert
        result.TotalLinesCleared.Should().Be(5);
    }

    [Fact]
    public void ClearLines_Should_Handle_Full_Board_Clear()
    {
        // Arrange
        var board = new GameBoard();
        // Fill entire board
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                board.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
            }
        }

        // Act
        var result = LineClearer.ClearLines(board);

        // Assert
        result.RowsCleared.Should().HaveCount(10);
        result.ColumnsCleared.Should().HaveCount(10);
        result.TotalLinesCleared.Should().Be(20);

        // Verify entire board is cleared
        for (int row = 0; row < 10; row++)
        {
            for (int col = 0; col < 10; col++)
            {
                board.GetCell(row, col).Should().BeNull();
            }
        }
    }
}
