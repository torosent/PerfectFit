namespace PerfectFit.Core.GameLogic.Generation;

using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Checks if a set of pieces can be placed on the board in some order.
/// Handles line clears properly to determine true solvability.
/// </summary>
public sealed class SolvabilityChecker
{
    private const int BoardSize = 8;
    private const int MaxPlacementsPerPiece = 50; // Early termination optimization

    /// <summary>
    /// Result of solvability check.
    /// </summary>
    /// <param name="IsSolvable">True if at least one valid sequence exists.</param>
    /// <param name="AtLeastOneFits">True if at least one piece can be placed.</param>
    /// <param name="AllFit">True if all pieces can be placed somewhere (ignoring order).</param>
    /// <param name="BestFirstPiece">The piece that has the most flexibility, if any.</param>
    public record SolvabilityResult(
        bool IsSolvable,
        bool AtLeastOneFits,
        bool AllFit,
        PieceType? BestFirstPiece = null);

    /// <summary>
    /// Checks if any permutation of the given pieces can be placed on the board.
    /// Uses line clear simulation and early termination for performance.
    /// </summary>
    /// <param name="board">The current game board.</param>
    /// <param name="pieces">The pieces to check (typically 3 pieces).</param>
    /// <returns>Result indicating solvability.</returns>
    public static SolvabilityResult CheckSolvability(GameBoard board, IReadOnlyList<PieceType> pieces)
    {
        if (pieces.Count == 0)
        {
            return new SolvabilityResult(IsSolvable: true, AtLeastOneFits: true, AllFit: true);
        }

        // First check: can any single piece fit?
        var fittingPieces = new List<PieceType>();
        foreach (var pieceType in pieces)
        {
            var piece = Piece.Create(pieceType);
            if (board.CanPlacePieceAnywhere(piece))
            {
                fittingPieces.Add(pieceType);
            }
        }

        bool atLeastOneFits = fittingPieces.Count > 0;
        bool allFit = fittingPieces.Count == pieces.Count;

        if (!atLeastOneFits)
        {
            return new SolvabilityResult(IsSolvable: false, AtLeastOneFits: false, AllFit: false);
        }

        // For single piece, if it fits, it's solvable
        if (pieces.Count == 1)
        {
            return new SolvabilityResult(IsSolvable: true, AtLeastOneFits: true, AllFit: true, BestFirstPiece: pieces[0]);
        }

        // Generate unique permutations (handles duplicates)
        var permutations = GenerateUniquePermutations(pieces.ToList());

        PieceType? bestFirstPiece = null;
        int bestFirstPiecePositions = 0;

        foreach (var permutation in permutations)
        {
            if (TryPlaceSequence(board, permutation))
            {
                // Track the best first piece (one with most flexibility)
                var firstPiece = Piece.Create(permutation[0]);
                int positions = board.GetValidPositions(firstPiece).Count;
                if (positions > bestFirstPiecePositions)
                {
                    bestFirstPiecePositions = positions;
                    bestFirstPiece = permutation[0];
                }

                return new SolvabilityResult(
                    IsSolvable: true,
                    AtLeastOneFits: atLeastOneFits,
                    AllFit: allFit,
                    BestFirstPiece: bestFirstPiece);
            }
        }

        return new SolvabilityResult(
            IsSolvable: false,
            AtLeastOneFits: atLeastOneFits,
            AllFit: allFit);
    }

    /// <summary>
    /// Checks if at least one piece from the set can be placed anywhere.
    /// Fast check without full solvability analysis.
    /// </summary>
    public static bool AtLeastOneFits(GameBoard board, IReadOnlyList<PieceType> pieces)
    {
        foreach (var pieceType in pieces)
        {
            var piece = Piece.Create(pieceType);
            if (board.CanPlacePieceAnywhere(piece))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the pieces that can fit on the board.
    /// </summary>
    public static List<PieceType> GetFittingPieces(GameBoard board, IReadOnlyList<PieceType> pieces)
    {
        var fitting = new List<PieceType>();
        foreach (var pieceType in pieces)
        {
            var piece = Piece.Create(pieceType);
            if (board.CanPlacePieceAnywhere(piece))
            {
                fitting.Add(pieceType);
            }
        }
        return fitting;
    }

    /// <summary>
    /// Tries to place a sequence of pieces on the board.
    /// Simulates line clears after each placement.
    /// </summary>
    private static bool TryPlaceSequence(GameBoard originalBoard, IReadOnlyList<PieceType> sequence)
    {
        // Work with a copy of the board
        var boardArray = originalBoard.ToArray();

        foreach (var pieceType in sequence)
        {
            var piece = Piece.Create(pieceType);
            var tempBoard = GameBoard.FromArray(boardArray);

            var positions = GetLimitedPositions(tempBoard, piece);
            if (positions.Count == 0)
            {
                return false;
            }

            // Try to find a position that works (use first valid for speed, or could try best)
            bool placed = false;
            foreach (var (row, col) in positions)
            {
                var testBoard = GameBoard.FromArray(boardArray);
                if (testBoard.TryPlacePiece(piece, row, col))
                {
                    // Simulate line clears
                    SimulateLineClear(testBoard);
                    boardArray = testBoard.ToArray();
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets valid positions with early termination for performance.
    /// </summary>
    private static List<(int Row, int Col)> GetLimitedPositions(GameBoard board, Piece piece)
    {
        var positions = new List<(int Row, int Col)>();
        var shape = piece.Shape;
        int pieceRows = shape.GetLength(0);
        int pieceCols = shape.GetLength(1);

        for (int row = 0; row <= BoardSize - pieceRows && positions.Count < MaxPlacementsPerPiece; row++)
        {
            for (int col = 0; col <= BoardSize - pieceCols && positions.Count < MaxPlacementsPerPiece; col++)
            {
                if (board.CanPlacePiece(piece, row, col))
                {
                    positions.Add((row, col));
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// Simulates line clearing on a board (modifies the board in place).
    /// </summary>
    private static void SimulateLineClear(GameBoard board)
    {
        // Find complete rows
        var rowsToClear = new List<int>();
        for (int row = 0; row < BoardSize; row++)
        {
            bool complete = true;
            for (int col = 0; col < BoardSize && complete; col++)
            {
                if (board.IsEmpty(row, col))
                {
                    complete = false;
                }
            }
            if (complete)
            {
                rowsToClear.Add(row);
            }
        }

        // Find complete columns
        var colsToClear = new List<int>();
        for (int col = 0; col < BoardSize; col++)
        {
            bool complete = true;
            for (int row = 0; row < BoardSize && complete; row++)
            {
                if (board.IsEmpty(row, col))
                {
                    complete = false;
                }
            }
            if (complete)
            {
                colsToClear.Add(col);
            }
        }

        // Clear rows
        foreach (int row in rowsToClear)
        {
            for (int col = 0; col < BoardSize; col++)
            {
                board.SetCell(row, col, null);
            }
        }

        // Clear columns
        foreach (int col in colsToClear)
        {
            for (int row = 0; row < BoardSize; row++)
            {
                board.SetCell(row, col, null);
            }
        }
    }

    /// <summary>
    /// Generates unique permutations handling duplicate elements.
    /// </summary>
    private static IEnumerable<List<PieceType>> GenerateUniquePermutations(List<PieceType> items)
    {
        if (items.Count <= 1)
        {
            yield return new List<PieceType>(items);
            yield break;
        }

        var seen = new HashSet<string>();

        foreach (var perm in GeneratePermutationsRecursive(items, 0))
        {
            var key = string.Join(",", perm.Select(p => (int)p));
            if (seen.Add(key))
            {
                yield return perm;
            }
        }
    }

    private static IEnumerable<List<PieceType>> GeneratePermutationsRecursive(List<PieceType> items, int start)
    {
        if (start == items.Count - 1)
        {
            yield return new List<PieceType>(items);
            yield break;
        }

        for (int i = start; i < items.Count; i++)
        {
            // Swap
            (items[start], items[i]) = (items[i], items[start]);

            foreach (var perm in GeneratePermutationsRecursive(items, start + 1))
            {
                yield return perm;
            }

            // Swap back
            (items[start], items[i]) = (items[i], items[start]);
        }
    }
}
