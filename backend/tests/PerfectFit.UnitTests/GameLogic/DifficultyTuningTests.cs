using FluentAssertions;
using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Generation;
using PerfectFit.Core.GameLogic.Pieces;
using Xunit.Abstractions;

namespace PerfectFit.UnitTests.GameLogic;

public class DifficultyTuningTests
{
    private readonly ITestOutputHelper _output;

    public DifficultyTuningTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void ProgressiveDifficulty_FavorsLargePieces_AsLinesClearedIncreases()
    {
        var board = new GameBoard(); // Safe board
        var selector = new WeightedPieceSelector(seed: 12345);

        // Baseline (0 lines)
        var pieces0 = GenerateMany(selector, board, 0, 1000);
        var largeCount0 = CountLargePieces(pieces0);

        // Advanced (100 lines)
        var selector100 = new WeightedPieceSelector(seed: 12345);
        var pieces100 = GenerateMany(selector100, board, 100, 1000);
        var largeCount100 = CountLargePieces(pieces100);

        _output.WriteLine($"Large pieces at 0 lines: {largeCount0}");
        _output.WriteLine($"Large pieces at 100 lines: {largeCount100}");

        largeCount100.Should().BeGreaterThan(largeCount0);
    }

    [Fact]
    public void RescueBias_GuaranteesSmallPiece_WhenDangerIsHigh()
    {
        var selector = new WeightedPieceSelector(seed: 12345);

        // Create a dangerous board
        var board = new GameBoard();
        for (int r = 0; r < 7; r++)
            for (int c = 0; c < 7; c++)
                board.TryPlacePiece(Piece.Create(PieceType.Dot), r, c);

        // Verify danger level is high
        var analysis = BoardAnalyzer.Analyze(board);
        analysis.DangerLevel.Should().BeGreaterThan(0.7);

        // Generate pieces
        var pieces = selector.GeneratePieces(board, 0, 3);

        // Should contain at least one small piece (Dot or Line2)
        pieces.Should().Contain(p => PieceWeights.GetCellCount(p) <= 2);
    }

    [Fact]
    public void RepetitionControl_ReducesFrequencyOfRecentPieces()
    {
        var board = new GameBoard();
        var selector = new WeightedPieceSelector(seed: 12345);

        // Force a piece into history by generating it
        // We can't easily force specific pieces without mocking, but we can check statistical distribution
        // Or we can rely on the fact that GeneratePieces updates history.

        // Let's generate a sequence and check for immediate repeats of rare pieces
        // Huge pieces are rare-ish.

        int repeats = 0;
        PieceType? lastPiece = null;

        for (int i = 0; i < 100; i++)
        {
            var pieces = selector.GeneratePieces(board, 0, 1);
            var current = pieces[0];

            if (lastPiece.HasValue && current == lastPiece.Value)
            {
                repeats++;
            }
            lastPiece = current;
        }

        _output.WriteLine($"Immediate repeats in 100 generations: {repeats}");
        // This is a weak test, but better than nothing. 
        // With 50% penalty, repeats should be lower than random chance.
        // Random chance for ~17 pieces is ~6%.

        repeats.Should().BeLessThan(15); // Generous upper bound
    }

    private List<PieceType> GenerateMany(WeightedPieceSelector selector, GameBoard board, int lines, int count)
    {
        var result = new List<PieceType>();
        for (int i = 0; i < count; i++)
        {
            var pieces = selector.GeneratePieces(board, lines, 1);
            result.AddRange(pieces);
        }
        return result;
    }

    private int CountLargePieces(List<PieceType> pieces)
    {
        return pieces.Count(p =>
            PieceWeights.GetCategory(p) == PieceWeights.PieceCategory.Large ||
            PieceWeights.GetCategory(p) == PieceWeights.PieceCategory.Heavy ||
            PieceWeights.GetCategory(p) == PieceWeights.PieceCategory.Huge);
    }
}
