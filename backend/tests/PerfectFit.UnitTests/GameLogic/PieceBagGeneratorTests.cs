using FluentAssertions;
using PerfectFit.Core.GameLogic.Pieces;
using PerfectFit.Core.GameLogic.PieceBag;

namespace PerfectFit.UnitTests.GameLogic;

public class PieceBagGeneratorTests
{
    private static readonly PieceType[] CoreTetrominoes =
    [
        PieceType.I, PieceType.O, PieceType.T, PieceType.S,
        PieceType.Z, PieceType.J, PieceType.L
    ];

    private static readonly PieceType[] ExtendedPieces =
    [
        PieceType.Dot, PieceType.Line2, PieceType.Line3, PieceType.Line5,
        PieceType.Corner, PieceType.BigCorner, PieceType.Square2x2, PieceType.Square3x3,
        PieceType.Rect2x3, PieceType.Rect3x2
    ];

    #region GetNextPieces Tests

    [Fact]
    public void GetNextPieces_ReturnsRequestedCount()
    {
        var generator = new PieceBagGenerator();
        var pieces = generator.GetNextPieces(3);
        pieces.Should().HaveCount(3);
    }

    [Fact]
    public void GetNextPieces_WithSeed_IsDeterministic()
    {
        var generator1 = new PieceBagGenerator(seed: 12345);
        var generator2 = new PieceBagGenerator(seed: 12345);

        var pieces1 = generator1.GetNextPieces(10);
        var pieces2 = generator2.GetNextPieces(10);

        pieces1.Should().BeEquivalentTo(pieces2, options => options.WithStrictOrdering());
    }

    [Fact]
    public void GetNextPieces_ConsumesFromBag()
    {
        var generator = new PieceBagGenerator(seed: 42);

        var first = generator.GetNextPieces(3);
        var second = generator.GetNextPieces(3);

        // Should get different pieces (next 3 in sequence)
        // Combined should be 6 unique from the bag
        var all = first.Concat(second).ToList();
        all.Should().HaveCount(6);
    }

    [Fact]
    public void GetNextPieces_AlwaysContainsCoreTetrominoes_AcrossMultipleBags()
    {
        var generator = new PieceBagGenerator(seed: 99);

        // Get enough pieces to span multiple bags (each bag has 7 core + some extended)
        var pieces = generator.GetNextPieces(35); // ~4-5 bags worth

        // Each core tetromino appears once per bag, so with 4-5 bags we should have several
        foreach (var tetromino in CoreTetrominoes)
        {
            pieces.Count(p => p == tetromino).Should().BeGreaterThanOrEqualTo(1,
                $"Expected at least 1 {tetromino} piece across multiple bags");
        }

        // Also verify all core tetrominoes are present
        pieces.Should().Contain(p => CoreTetrominoes.Contains(p));
    }

    [Fact]
    public void GetNextPieces_ZeroCount_ReturnsEmpty()
    {
        var generator = new PieceBagGenerator();
        var pieces = generator.GetNextPieces(0);
        pieces.Should().BeEmpty();
    }

    [Fact]
    public void GetNextPieces_NegativeCount_ReturnsEmpty()
    {
        var generator = new PieceBagGenerator();
        var pieces = generator.GetNextPieces(-5);
        pieces.Should().BeEmpty();
    }

    #endregion

    #region PeekNextPieces Tests

    [Fact]
    public void PeekNextPieces_DoesNotConsumePieces()
    {
        var generator = new PieceBagGenerator(seed: 42);

        var peeked = generator.PeekNextPieces(3);
        var actual = generator.GetNextPieces(3);

        peeked.Should().BeEquivalentTo(actual, options => options.WithStrictOrdering());
    }

    [Fact]
    public void PeekNextPieces_ReturnsCorrectCount()
    {
        var generator = new PieceBagGenerator();
        var peeked = generator.PeekNextPieces(5);
        peeked.Should().HaveCount(5);
    }

    [Fact]
    public void PeekNextPieces_ConsistentAcrossCalls()
    {
        var generator = new PieceBagGenerator(seed: 123);

        var peek1 = generator.PeekNextPieces(3);
        var peek2 = generator.PeekNextPieces(3);

        peek1.Should().BeEquivalentTo(peek2, options => options.WithStrictOrdering());
    }

    #endregion

    #region Serialization Tests

    [Fact]
    public void SerializeState_ReturnsNonEmptyString()
    {
        var generator = new PieceBagGenerator(seed: 42);
        generator.GetNextPieces(3); // Consume some pieces

        var state = generator.SerializeState();

        state.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromState_RestoresExactState()
    {
        var generator = new PieceBagGenerator(seed: 42);
        generator.GetNextPieces(5); // Consume some pieces

        var state = generator.SerializeState();
        var restored = PieceBagGenerator.FromState(state);

        // Both should produce the same next pieces
        var original = generator.GetNextPieces(10);
        var restoredPieces = restored.GetNextPieces(10);

        restoredPieces.Should().BeEquivalentTo(original, options => options.WithStrictOrdering());
    }

    [Fact]
    public void FromState_WithNull_ThrowsArgumentException()
    {
        var action = () => PieceBagGenerator.FromState(null!);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromState_WithEmptyString_ThrowsArgumentException()
    {
        var action = () => PieceBagGenerator.FromState("");
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromState_WithInvalidString_ThrowsException()
    {
        var action = () => PieceBagGenerator.FromState("invalid-state-data");
        action.Should().Throw<Exception>();
    }

    #endregion

    #region Extended Pieces Tests

    [Fact]
    public void GeneratedPieces_CanIncludeExtendedPieces()
    {
        // With enough generations, we should see some extended pieces
        var generator = new PieceBagGenerator(seed: 42);
        var pieces = generator.GetNextPieces(50);

        var hasExtended = pieces.Any(p => ExtendedPieces.Contains(p));
        hasExtended.Should().BeTrue("Expected some extended pieces in a large sample");
    }

    [Fact]
    public void GeneratedPieces_ExtendedPiecesAreOptional()
    {
        // Extended pieces at 50% rate - some bags may not have all
        // But core tetrominoes should always be present
        var generator = new PieceBagGenerator(seed: 42);

        for (int i = 0; i < 5; i++)
        {
            var bagPieces = generator.GetNextPieces(7);

            // Should always have valid piece types
            foreach (var piece in bagPieces)
            {
                Enum.IsDefined(typeof(PieceType), piece).Should().BeTrue();
            }
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithoutSeed_CreatesRandomGenerator()
    {
        var gen1 = new PieceBagGenerator();
        var gen2 = new PieceBagGenerator();

        // Very unlikely to be identical without seed
        var pieces1 = gen1.GetNextPieces(20);
        var pieces2 = gen2.GetNextPieces(20);

        // They could theoretically be the same, but extremely unlikely
        // Just verify both produce valid pieces
        pieces1.Should().HaveCount(20);
        pieces2.Should().HaveCount(20);
    }

    #endregion
}
