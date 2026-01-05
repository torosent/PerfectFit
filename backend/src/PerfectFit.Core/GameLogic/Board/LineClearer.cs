namespace PerfectFit.Core.GameLogic.Board;

/// <summary>
/// Result of a line clearing operation.
/// </summary>
/// <param name="RowsCleared">The indices of rows that were cleared.</param>
/// <param name="ColumnsCleared">The indices of columns that were cleared.</param>
public sealed record ClearResult(IReadOnlyList<int> RowsCleared, IReadOnlyList<int> ColumnsCleared)
{
    /// <summary>
    /// Gets the total number of lines cleared (rows + columns).
    /// </summary>
    public int TotalLinesCleared => RowsCleared.Count + ColumnsCleared.Count;
}

/// <summary>
/// Handles line clearing logic for the game board.
/// Clears complete rows and columns without applying gravity.
/// </summary>
public static class LineClearer
{
    private const int BoardSize = 8;

    /// <summary>
    /// Checks for and clears any complete rows and columns on the board.
    /// Does NOT apply gravity - cells above cleared rows stay in place.
    /// </summary>
    /// <param name="board">The game board to check and modify.</param>
    /// <returns>A ClearResult indicating which rows and columns were cleared.</returns>
    public static ClearResult ClearLines(GameBoard board)
    {
        var rowsToCheck = FindCompleteRows(board);
        var colsToCheck = FindCompleteColumns(board);

        // Clear the rows
        foreach (int row in rowsToCheck)
        {
            ClearRow(board, row);
        }

        // Clear the columns
        foreach (int col in colsToCheck)
        {
            ClearColumn(board, col);
        }

        return new ClearResult(rowsToCheck, colsToCheck);
    }

    private static List<int> FindCompleteRows(GameBoard board)
    {
        var completeRows = new List<int>();

        for (int row = 0; row < BoardSize; row++)
        {
            bool isComplete = true;
            for (int col = 0; col < BoardSize; col++)
            {
                if (board.GetCell(row, col) == null)
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
            {
                completeRows.Add(row);
            }
        }

        return completeRows;
    }

    private static List<int> FindCompleteColumns(GameBoard board)
    {
        var completeCols = new List<int>();

        for (int col = 0; col < BoardSize; col++)
        {
            bool isComplete = true;
            for (int row = 0; row < BoardSize; row++)
            {
                if (board.GetCell(row, col) == null)
                {
                    isComplete = false;
                    break;
                }
            }

            if (isComplete)
            {
                completeCols.Add(col);
            }
        }

        return completeCols;
    }

    private static void ClearRow(GameBoard board, int row)
    {
        for (int col = 0; col < BoardSize; col++)
        {
            board.SetCell(row, col, null);
        }
    }

    private static void ClearColumn(GameBoard board, int col)
    {
        for (int row = 0; row < BoardSize; row++)
        {
            board.SetCell(row, col, null);
        }
    }
}
