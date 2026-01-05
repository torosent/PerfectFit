namespace PerfectFit.Core.GameLogic.Generation;

using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Analyzes board state to calculate danger level and identify helpful placements.
/// Used to dynamically adjust piece generation weights.
/// </summary>
public sealed class BoardAnalyzer
{
    private const int BoardSize = 8;
    private const int TotalCells = BoardSize * BoardSize;

    /// <summary>
    /// Result of board analysis.
    /// </summary>
    /// <param name="DangerLevel">Danger level from 0.0 (safe) to 1.0 (critical).</param>
    /// <param name="EmptyCells">Number of empty cells on the board.</param>
    /// <param name="TotalLegalMoves">Total number of legal placements for common pieces.</param>
    /// <param name="NearCompleteRows">Rows that need only a few cells to complete.</param>
    /// <param name="NearCompleteColumns">Columns that need only a few cells to complete.</param>
    public record BoardAnalysis(
        double DangerLevel,
        int EmptyCells,
        int TotalLegalMoves,
        IReadOnlyList<int> NearCompleteRows,
        IReadOnlyList<int> NearCompleteColumns);

    /// <summary>
    /// Analyzes the current board state and calculates a danger level.
    /// </summary>
    /// <param name="board">The game board to analyze.</param>
    /// <returns>Analysis result with danger level and board metrics.</returns>
    public static BoardAnalysis Analyze(GameBoard board)
    {
        var emptyCells = CountEmptyCells(board);
        var legalMoves = CountLegalMoves(board);
        var nearCompleteRows = FindNearCompleteRows(board);
        var nearCompleteCols = FindNearCompleteColumns(board);

        var dangerLevel = CalculateDangerLevel(emptyCells, legalMoves, nearCompleteRows.Count, nearCompleteCols.Count);

        return new BoardAnalysis(
            DangerLevel: dangerLevel,
            EmptyCells: emptyCells,
            TotalLegalMoves: legalMoves,
            NearCompleteRows: nearCompleteRows,
            NearCompleteColumns: nearCompleteCols);
    }

    /// <summary>
    /// Counts empty cells on the board.
    /// </summary>
    public static int CountEmptyCells(GameBoard board)
    {
        int count = 0;
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                if (board.IsEmpty(row, col))
                {
                    count++;
                }
            }
        }
        return count;
    }

    /// <summary>
    /// Counts total legal placements for a representative set of pieces.
    /// Uses common pieces to estimate board openness without checking all pieces.
    /// </summary>
    public static int CountLegalMoves(GameBoard board)
    {
        // Check representative pieces from different size categories
        var representativePieces = new[] { PieceType.Dot, PieceType.Line2, PieceType.Line3, PieceType.T, PieceType.Square2x2 };

        int totalMoves = 0;
        foreach (var pieceType in representativePieces)
        {
            var piece = Piece.Create(pieceType);
            var positions = board.GetValidPositions(piece);
            totalMoves += positions.Count;
        }

        return totalMoves;
    }

    /// <summary>
    /// Finds rows that are nearly complete (1-3 cells remaining).
    /// </summary>
    public static List<int> FindNearCompleteRows(GameBoard board, int maxMissingCells = 3)
    {
        var nearComplete = new List<int>();

        for (int row = 0; row < BoardSize; row++)
        {
            int emptyInRow = 0;
            for (int col = 0; col < BoardSize; col++)
            {
                if (board.IsEmpty(row, col))
                {
                    emptyInRow++;
                }
            }

            if (emptyInRow > 0 && emptyInRow <= maxMissingCells)
            {
                nearComplete.Add(row);
            }
        }

        return nearComplete;
    }

    /// <summary>
    /// Finds columns that are nearly complete (1-3 cells remaining).
    /// </summary>
    public static List<int> FindNearCompleteColumns(GameBoard board, int maxMissingCells = 3)
    {
        var nearComplete = new List<int>();

        for (int col = 0; col < BoardSize; col++)
        {
            int emptyInCol = 0;
            for (int row = 0; row < BoardSize; row++)
            {
                if (board.IsEmpty(row, col))
                {
                    emptyInCol++;
                }
            }

            if (emptyInCol > 0 && emptyInCol <= maxMissingCells)
            {
                nearComplete.Add(col);
            }
        }

        return nearComplete;
    }

    /// <summary>
    /// Calculates danger level based on multiple factors.
    /// </summary>
    /// <param name="emptyCells">Number of empty cells.</param>
    /// <param name="legalMoves">Total legal moves available.</param>
    /// <param name="nearCompleteRows">Number of rows close to completion.</param>
    /// <param name="nearCompleteCols">Number of columns close to completion.</param>
    /// <returns>Danger level from 0.0 to 1.0.</returns>
    private static double CalculateDangerLevel(int emptyCells, int legalMoves, int nearCompleteRows, int nearCompleteCols)
    {
        // Factor 1: Cell occupancy (0 = empty board, 1 = full board)
        double occupancyDanger = 1.0 - ((double)emptyCells / TotalCells);

        // Factor 2: Legal moves scarcity
        // Estimate: A good open board might have ~200+ legal moves for common pieces
        // A dangerous board might have <50
        const int maxExpectedMoves = 250;
        const int criticalMovesThreshold = 30;
        double movesDanger;
        if (legalMoves <= criticalMovesThreshold)
        {
            movesDanger = 1.0;
        }
        else
        {
            movesDanger = 1.0 - Math.Min(1.0, (double)legalMoves / maxExpectedMoves);
        }

        // Factor 3: Near-complete lines (opportunity vs risk)
        // Having near-complete lines is actually GOOD if player can clear them
        // But if there are too many, it indicates a fragmented board
        double lineFragmentationDanger = (nearCompleteRows + nearCompleteCols) > 6 ? 0.3 : 0.0;

        // Combine factors with weights
        // Occupancy is the primary indicator
        // Legal moves provides nuance (can have lots of empty cells but in unhelpful patterns)
        double dangerLevel =
            0.50 * occupancyDanger +
            0.40 * movesDanger +
            0.10 * lineFragmentationDanger;

        // Apply non-linear scaling: Make high danger levels more pronounced
        // This ensures we don't start helping too early
        dangerLevel = Math.Pow(dangerLevel, 1.5);

        return Math.Clamp(dangerLevel, 0.0, 1.0);
    }

    /// <summary>
    /// Checks if placing a piece at a position would contribute to line clearing.
    /// </summary>
    /// <param name="board">The game board.</param>
    /// <param name="piece">The piece to check.</param>
    /// <param name="startRow">Starting row position.</param>
    /// <param name="startCol">Starting column position.</param>
    /// <returns>True if placement contributes to clearing potential.</returns>
    public static bool ContributesToLineClear(GameBoard board, Piece piece, int startRow, int startCol)
    {
        if (!board.CanPlacePiece(piece, startRow, startCol))
            return false;

        // Check which rows and columns the piece touches
        var shape = piece.Shape;
        int rows = shape.GetLength(0);
        int cols = shape.GetLength(1);

        var touchedRows = new HashSet<int>();
        var touchedCols = new HashSet<int>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (shape[r, c])
                {
                    touchedRows.Add(startRow + r);
                    touchedCols.Add(startCol + c);
                }
            }
        }

        // Check if any touched row would become complete
        foreach (int row in touchedRows)
        {
            int emptyCellsInRow = 0;
            for (int col = 0; col < BoardSize; col++)
            {
                if (board.IsEmpty(row, col))
                {
                    emptyCellsInRow++;
                }
            }

            // Would the piece fill enough cells to complete this row?
            int cellsFilledByPieceInRow = 0;
            for (int c = 0; c < cols; c++)
            {
                if (shape[row - startRow >= 0 && row - startRow < rows ? row - startRow : -1, c])
                {
                    if (board.IsEmpty(row, startCol + c))
                    {
                        cellsFilledByPieceInRow++;
                    }
                }
            }

            if (emptyCellsInRow <= cellsFilledByPieceInRow)
            {
                return true;
            }
        }

        // Similar check for columns
        foreach (int col in touchedCols)
        {
            int emptyCellsInCol = 0;
            for (int row = 0; row < BoardSize; row++)
            {
                if (board.IsEmpty(row, col))
                {
                    emptyCellsInCol++;
                }
            }

            int cellsFilledByPieceInCol = 0;
            for (int r = 0; r < rows; r++)
            {
                if (shape[r, col - startCol >= 0 && col - startCol < cols ? col - startCol : -1])
                {
                    if (board.IsEmpty(startRow + r, col))
                    {
                        cellsFilledByPieceInCol++;
                    }
                }
            }

            if (emptyCellsInCol <= cellsFilledByPieceInCol)
            {
                return true;
            }
        }

        return false;
    }
}
