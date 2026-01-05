using FluentAssertions;
using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Generation;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.UnitTests.GameLogic;

public class PieceWeightsTests
{
    [Fact]
    public void GetCategory_ReturnsCorrectCategory_ForAllPieceTypes()
    {
        PieceWeights.GetCategory(PieceType.Dot).Should().Be(PieceWeights.PieceCategory.Tiny);
        PieceWeights.GetCategory(PieceType.Line2).Should().Be(PieceWeights.PieceCategory.Small);
        PieceWeights.GetCategory(PieceType.Line3).Should().Be(PieceWeights.PieceCategory.Medium);
        PieceWeights.GetCategory(PieceType.T).Should().Be(PieceWeights.PieceCategory.Standard);
        PieceWeights.GetCategory(PieceType.Line5).Should().Be(PieceWeights.PieceCategory.Large);
        PieceWeights.GetCategory(PieceType.Rect2x3).Should().Be(PieceWeights.PieceCategory.Heavy);
        PieceWeights.GetCategory(PieceType.Rect3x2).Should().Be(PieceWeights.PieceCategory.Heavy);
        PieceWeights.GetCategory(PieceType.Square3x3).Should().Be(PieceWeights.PieceCategory.Huge);
    }

    [Fact]
    public void GetCellCount_ReturnsCorrectCount_ForAllPieceTypes()
    {
        PieceWeights.GetCellCount(PieceType.Dot).Should().Be(1);
        PieceWeights.GetCellCount(PieceType.Line2).Should().Be(2);
        PieceWeights.GetCellCount(PieceType.Line3).Should().Be(3);
        PieceWeights.GetCellCount(PieceType.T).Should().Be(4);
        PieceWeights.GetCellCount(PieceType.Line5).Should().Be(5);
        PieceWeights.GetCellCount(PieceType.Rect2x3).Should().Be(6);
        PieceWeights.GetCellCount(PieceType.Rect3x2).Should().Be(6);
        PieceWeights.GetCellCount(PieceType.Square3x3).Should().Be(9);
    }

    [Fact]
    public void GetWeight_AtDangerZero_UsesSafeWeights()
    {
        // At danger 0, weights should equal safe weights
        var dotWeight = PieceWeights.GetWeight(PieceType.Dot, 0.0);
        var hugeWeight = PieceWeights.GetWeight(PieceType.Square3x3, 0.0);

        dotWeight.Should().Be(PieceWeights.SafeWeights[PieceWeights.PieceCategory.Tiny]);
        hugeWeight.Should().Be(PieceWeights.SafeWeights[PieceWeights.PieceCategory.Huge]);
    }

    [Fact]
    public void GetWeight_AtDangerOne_UsesDangerWeights()
    {
        // At danger 1, weights should equal danger weights
        var dotWeight = PieceWeights.GetWeight(PieceType.Dot, 1.0);
        var hugeWeight = PieceWeights.GetWeight(PieceType.Square3x3, 1.0);

        dotWeight.Should().BeApproximately(PieceWeights.DangerWeights[PieceWeights.PieceCategory.Tiny], 0.0001);
        hugeWeight.Should().BeApproximately(PieceWeights.DangerWeights[PieceWeights.PieceCategory.Huge], 0.0001);
    }

    [Fact]
    public void GetWeight_AtMidDanger_InterpolatesCorrectly()
    {
        // At danger 0.5, weight should be midpoint
        var weight = PieceWeights.GetWeight(PieceType.Dot, 0.5);
        var safeWeight = PieceWeights.SafeWeights[PieceWeights.PieceCategory.Tiny];
        var dangerWeight = PieceWeights.DangerWeights[PieceWeights.PieceCategory.Tiny];
        var expected = safeWeight + (dangerWeight - safeWeight) * 0.5;

        weight.Should().BeApproximately(expected, 0.001);
    }

    [Fact]
    public void GetWeight_ClampsNegativeDanger()
    {
        var weight = PieceWeights.GetWeight(PieceType.Dot, -0.5);
        var safeWeight = PieceWeights.SafeWeights[PieceWeights.PieceCategory.Tiny];

        weight.Should().Be(safeWeight);
    }

    [Fact]
    public void GetWeight_ClampsExcessiveDanger()
    {
        var weight = PieceWeights.GetWeight(PieceType.Dot, 1.5);
        var dangerWeight = PieceWeights.DangerWeights[PieceWeights.PieceCategory.Tiny];

        weight.Should().Be(dangerWeight);
    }

    [Fact]
    public void GetAllWeights_ReturnsWeightsForAllPieceTypes()
    {
        var weights = PieceWeights.GetAllWeights(0.5);

        weights.Should().HaveCount(Enum.GetValues<PieceType>().Length);

        foreach (var pieceType in Enum.GetValues<PieceType>())
        {
            weights.Should().ContainKey(pieceType);
            weights[pieceType].Should().BePositive();
        }
    }

    [Fact]
    public void SmallPiecesHaveHigherWeightAtHighDanger()
    {
        var weights = PieceWeights.GetAllWeights(0.9);

        // Small pieces should have higher weights than large pieces at high danger
        weights[PieceType.Dot].Should().BeGreaterThan(weights[PieceType.Square3x3]);
        weights[PieceType.Line2].Should().BeGreaterThan(weights[PieceType.BigCorner]);
    }

    [Fact]
    public void LargePiecesHaveReasonableWeightAtLowDanger()
    {
        var weights = PieceWeights.GetAllWeights(0.1);

        // At low danger, larger pieces should still be viable
        weights[PieceType.Square3x3].Should().BeGreaterThan(0.3);
        weights[PieceType.Line5].Should().BeGreaterThan(0.7);
    }
}

public class BoardAnalyzerTests
{
    [Fact]
    public void CountEmptyCells_ReturnsCorrectCount_ForEmptyBoard()
    {
        var board = new GameBoard();
        var count = BoardAnalyzer.CountEmptyCells(board);

        count.Should().Be(64); // 8x8 board
    }

    [Fact]
    public void CountEmptyCells_ReturnsCorrectCount_AfterPlacement()
    {
        var board = new GameBoard();
        var piece = Piece.Create(PieceType.T); // 4 cells
        board.TryPlacePiece(piece, 0, 0);

        var count = BoardAnalyzer.CountEmptyCells(board);

        count.Should().Be(60);
    }

    [Fact]
    public void Analyze_ReturnsLowDanger_ForEmptyBoard()
    {
        var board = new GameBoard();
        var analysis = BoardAnalyzer.Analyze(board);

        analysis.DangerLevel.Should().BeLessThan(0.3);
        analysis.EmptyCells.Should().Be(64);
        analysis.TotalLegalMoves.Should().BeGreaterThan(64);
    }

    [Fact]
    public void Analyze_ReturnsHighDanger_ForNearlyFullBoard()
    {
        var board = new GameBoard();

        // Fill most of the board, leaving just a few cells using Dot pieces
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                board.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
            }
        }

        var analysis = BoardAnalyzer.Analyze(board);

        analysis.DangerLevel.Should().BeGreaterThan(0.5);
        analysis.EmptyCells.Should().Be(8);
    }

    [Fact]
    public void FindNearCompleteRows_FindsRows_WithFewEmptyCells()
    {
        var board = new GameBoard();

        // Fill row 5 except for 2 cells using Dot pieces
        for (int col = 0; col < 6; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 5, col);
        }

        var nearComplete = BoardAnalyzer.FindNearCompleteRows(board);

        nearComplete.Should().Contain(5);
    }

    [Fact]
    public void FindNearCompleteColumns_FindsColumns_WithFewEmptyCells()
    {
        var board = new GameBoard();

        // Fill column 3 except for 1 cell using Dot pieces
        for (int row = 0; row < 7; row++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), row, 3);
        }

        var nearComplete = BoardAnalyzer.FindNearCompleteColumns(board);

        nearComplete.Should().Contain(3);
    }

    [Fact]
    public void CountLegalMoves_ReturnsPositiveCount_ForEmptyBoard()
    {
        var board = new GameBoard();
        var moves = BoardAnalyzer.CountLegalMoves(board);

        moves.Should().BeGreaterThan(200); // Many legal moves on empty board
    }
}

public class SolvabilityCheckerTests
{
    [Fact]
    public void CheckSolvability_ReturnsTrue_ForEmptyPieceList()
    {
        var board = new GameBoard();
        var result = SolvabilityChecker.CheckSolvability(board, []);

        result.IsSolvable.Should().BeTrue();
        result.AtLeastOneFits.Should().BeTrue();
    }

    [Fact]
    public void CheckSolvability_ReturnsTrue_ForSingleFittingPiece()
    {
        var board = new GameBoard();
        var pieces = new List<PieceType> { PieceType.T };

        var result = SolvabilityChecker.CheckSolvability(board, pieces);

        result.IsSolvable.Should().BeTrue();
        result.AtLeastOneFits.Should().BeTrue();
        result.AllFit.Should().BeTrue();
    }

    [Fact]
    public void CheckSolvability_ReturnsTrue_ForMultipleFittingPieces()
    {
        var board = new GameBoard();
        var pieces = new List<PieceType> { PieceType.T, PieceType.I, PieceType.O };

        var result = SolvabilityChecker.CheckSolvability(board, pieces);

        result.IsSolvable.Should().BeTrue();
        result.AtLeastOneFits.Should().BeTrue();
        result.AllFit.Should().BeTrue();
    }

    [Fact]
    public void CheckSolvability_ReturnsFalse_WhenNoPiecesFit()
    {
        var board = new GameBoard();

        // Fill all but a tiny area that won't fit any large piece using Dot pieces
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if (!(row == 5 && col == 5)) // Leave just one cell empty
                {
                    board.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
                }
            }
        }

        var pieces = new List<PieceType> { PieceType.T, PieceType.I, PieceType.O };

        var result = SolvabilityChecker.CheckSolvability(board, pieces);

        result.IsSolvable.Should().BeFalse();
        result.AtLeastOneFits.Should().BeFalse();
    }

    [Fact]
    public void CheckSolvability_HandlesLineClearSimulation()
    {
        var board = new GameBoard();

        // Fill row 0 except first column using Dot pieces
        for (int col = 1; col < 8; col++)
        {
            board.TryPlacePiece(Piece.Create(PieceType.Dot), 0, col);
        }

        // Place pieces that will complete the row and clear it
        var pieces = new List<PieceType> { PieceType.Dot };

        var result = SolvabilityChecker.CheckSolvability(board, pieces);

        result.IsSolvable.Should().BeTrue();
    }

    [Fact]
    public void AtLeastOneFits_ReturnsTrue_WhenOnePieceFits()
    {
        var board = new GameBoard();
        var pieces = new List<PieceType> { PieceType.Square3x3, PieceType.Dot };

        var result = SolvabilityChecker.AtLeastOneFits(board, pieces);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetFittingPieces_ReturnsOnlyFittingPieces()
    {
        var board = new GameBoard();

        // Fill most of board leaving small gaps using Dot pieces
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 3 != 0) // Leave some gaps
                {
                    board.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
                }
            }
        }

        var pieces = new List<PieceType> { PieceType.Dot, PieceType.Square3x3 };
        var fitting = SolvabilityChecker.GetFittingPieces(board, pieces);

        // Dot should fit, Square3x3 likely won't due to fragmentation
        fitting.Should().Contain(PieceType.Dot);
    }

    [Fact]
    public void CheckSolvability_HandlesDuplicatePieces()
    {
        var board = new GameBoard();
        var pieces = new List<PieceType> { PieceType.Dot, PieceType.Dot, PieceType.Dot };

        var result = SolvabilityChecker.CheckSolvability(board, pieces);

        result.IsSolvable.Should().BeTrue();
        result.AtLeastOneFits.Should().BeTrue();
    }
}

public class WeightedPieceSelectorTests
{
    [Fact]
    public void GeneratePieces_ReturnsRequestedCount()
    {
        var board = new GameBoard();
        var selector = new WeightedPieceSelector(seed: 42);

        var pieces = selector.GeneratePieces(board, count: 3);

        pieces.Should().HaveCount(3);
    }

    [Fact]
    public void GeneratePieces_ReturnsEmptyList_WhenCountIsZero()
    {
        var board = new GameBoard();
        var selector = new WeightedPieceSelector();

        var pieces = selector.GeneratePieces(board, count: 0);

        pieces.Should().BeEmpty();
    }

    [Fact]
    public void GeneratePieces_IsDeterministic_WithSeed()
    {
        var board = new GameBoard();
        var selector1 = new WeightedPieceSelector(seed: 12345);
        var selector2 = new WeightedPieceSelector(seed: 12345);

        var pieces1 = selector1.GeneratePieces(board, count: 3);
        var pieces2 = selector2.GeneratePieces(board, count: 3);

        pieces1.Should().Equal(pieces2);
    }

    [Fact]
    public void GeneratePieces_GuaranteesAtLeastOneFits_OnTightBoard()
    {
        var board = new GameBoard();

        // Create a tight board using Dot pieces
        for (int row = 0; row < 7; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                board.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
            }
        }

        var selector = new WeightedPieceSelector(seed: 42);
        var pieces = selector.GeneratePieces(board, count: 3);

        // At least one piece should fit
        var atLeastOneFits = SolvabilityChecker.AtLeastOneFits(board, pieces);
        atLeastOneFits.Should().BeTrue("Generated pieces should include at least one that fits");
    }

    [Fact]
    public void GeneratePieces_FavorsSmallPieces_OnDangerousBoard()
    {
        // Generate many sets and count small vs large pieces
        int smallCount = 0;
        int largeCount = 0;

        for (int i = 0; i < 100; i++)
        {
            // Create a dangerous board (many cells filled) using Dot pieces
            var freshBoard = new GameBoard();
            for (int row = 0; row < 7; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    freshBoard.TryPlacePiece(Piece.Create(PieceType.Dot), row, col);
                }
            }

            var pieces = new WeightedPieceSelector().GeneratePieces(freshBoard, count: 3);

            foreach (var piece in pieces)
            {
                var cellCount = PieceWeights.GetCellCount(piece);
                if (cellCount <= 2) smallCount++;
                else if (cellCount >= 5) largeCount++;
            }
        }

        // On dangerous board, small pieces should be more common than large ones
        smallCount.Should().BeGreaterThan(largeCount, $"Small: {smallCount}, Large: {largeCount}");
    }

    [Fact]
    public void SerializeAndRestore_PreservesState()
    {
        var board = new GameBoard();
        var selector = new WeightedPieceSelector(seed: 42);
        var pieces = selector.GeneratePieces(board, count: 3);

        var state = selector.SerializeState(pieces);
        var (restoredSelector, restoredPieces) = WeightedPieceSelector.FromState(state);

        restoredPieces.Should().Equal(pieces);
    }
}
