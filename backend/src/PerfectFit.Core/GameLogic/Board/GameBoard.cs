using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.Core.GameLogic.Board;

/// <summary>
/// Represents a 10x10 game board for PerfectFit.
/// Each cell can contain a color string (indicating a placed piece) or null (empty).
/// </summary>
public sealed class GameBoard
{
    private const int BoardSize = 10;
    private readonly string?[,] _grid;

    /// <summary>
    /// Creates a new empty game board.
    /// </summary>
    public GameBoard()
    {
        _grid = new string?[BoardSize, BoardSize];
    }

    private GameBoard(string?[,] grid)
    {
        _grid = grid;
    }

    /// <summary>
    /// Gets the color at the specified cell, or null if empty.
    /// </summary>
    /// <param name="row">The row index (0-9).</param>
    /// <param name="col">The column index (0-9).</param>
    /// <returns>The color string or null.</returns>
    public string? GetCell(int row, int col)
    {
        if (!IsInBounds(row, col))
            return null;
        return _grid[row, col];
    }

    /// <summary>
    /// Checks if the specified position is within board bounds.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="col">The column index.</param>
    /// <returns>True if position is valid, false otherwise.</returns>
    public bool IsInBounds(int row, int col)
    {
        return row >= 0 && row < BoardSize && col >= 0 && col < BoardSize;
    }

    /// <summary>
    /// Checks if the specified cell is empty and within bounds.
    /// </summary>
    /// <param name="row">The row index.</param>
    /// <param name="col">The column index.</param>
    /// <returns>True if cell is empty and valid, false otherwise.</returns>
    public bool IsEmpty(int row, int col)
    {
        return IsInBounds(row, col) && _grid[row, col] == null;
    }

    /// <summary>
    /// Checks if a piece can be placed at the specified position.
    /// </summary>
    /// <param name="piece">The piece to place.</param>
    /// <param name="startRow">The top-left row position.</param>
    /// <param name="startCol">The top-left column position.</param>
    /// <returns>True if the piece can be placed, false otherwise.</returns>
    public bool CanPlacePiece(Piece piece, int startRow, int startCol)
    {
        var shape = piece.Shape;
        int rows = shape.GetLength(0);
        int cols = shape.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (shape[row, col])
                {
                    int boardRow = startRow + row;
                    int boardCol = startCol + col;

                    if (!IsInBounds(boardRow, boardCol) || !IsEmpty(boardRow, boardCol))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Attempts to place a piece at the specified position.
    /// </summary>
    /// <param name="piece">The piece to place.</param>
    /// <param name="startRow">The top-left row position.</param>
    /// <param name="startCol">The top-left column position.</param>
    /// <returns>True if the piece was placed, false otherwise.</returns>
    public bool TryPlacePiece(Piece piece, int startRow, int startCol)
    {
        if (!CanPlacePiece(piece, startRow, startCol))
        {
            return false;
        }

        var shape = piece.Shape;
        int rows = shape.GetLength(0);
        int cols = shape.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                if (shape[row, col])
                {
                    _grid[startRow + row, startCol + col] = piece.Color;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a piece can be placed anywhere on the board.
    /// </summary>
    /// <param name="piece">The piece to check.</param>
    /// <returns>True if there is at least one valid position, false otherwise.</returns>
    public bool CanPlacePieceAnywhere(Piece piece)
    {
        var shape = piece.Shape;
        int pieceRows = shape.GetLength(0);
        int pieceCols = shape.GetLength(1);

        for (int row = 0; row <= BoardSize - pieceRows; row++)
        {
            for (int col = 0; col <= BoardSize - pieceCols; col++)
            {
                if (CanPlacePiece(piece, row, col))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all valid positions where a piece can be placed.
    /// </summary>
    /// <param name="piece">The piece to check.</param>
    /// <returns>A list of valid (row, col) positions.</returns>
    public List<(int Row, int Col)> GetValidPositions(Piece piece)
    {
        var positions = new List<(int Row, int Col)>();
        var shape = piece.Shape;
        int pieceRows = shape.GetLength(0);
        int pieceCols = shape.GetLength(1);

        for (int row = 0; row <= BoardSize - pieceRows; row++)
        {
            for (int col = 0; col <= BoardSize - pieceCols; col++)
            {
                if (CanPlacePiece(piece, row, col))
                {
                    positions.Add((row, col));
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// Returns a copy of the board state as a 2D array.
    /// </summary>
    /// <returns>A copy of the internal grid.</returns>
    public string?[,] ToArray()
    {
        var copy = new string?[BoardSize, BoardSize];
        Array.Copy(_grid, copy, _grid.Length);
        return copy;
    }

    /// <summary>
    /// Creates a GameBoard from a 2D array.
    /// </summary>
    /// <param name="source">The source array (must be 10x10).</param>
    /// <returns>A new GameBoard instance.</returns>
    /// <exception cref="ArgumentException">If the array is not 10x10.</exception>
    public static GameBoard FromArray(string?[,] source)
    {
        if (source.GetLength(0) != BoardSize || source.GetLength(1) != BoardSize)
        {
            throw new ArgumentException($"Array must be {BoardSize}x{BoardSize}.", nameof(source));
        }

        var grid = new string?[BoardSize, BoardSize];
        Array.Copy(source, grid, source.Length);
        return new GameBoard(grid);
    }

    /// <summary>
    /// Sets a cell to a specific value. Internal use only (for clearing).
    /// </summary>
    internal void SetCell(int row, int col, string? value)
    {
        if (IsInBounds(row, col))
        {
            _grid[row, col] = value;
        }
    }
}
