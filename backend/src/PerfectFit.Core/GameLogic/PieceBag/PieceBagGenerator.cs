using System.Text.Json;
using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Generation;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.Core.GameLogic.PieceBag;

/// <summary>
/// State for serialization of the piece bag generator.
/// </summary>
internal record PieceBagState(
    List<PieceType> RemainingPieces,
    int? Seed,
    int RandomState,
    bool UseWeightedGeneration,
    string? WeightedSelectorState);

/// <summary>
/// Generates pieces using either weighted board-aware generation or classic 7-bag randomizer.
/// 
/// Weighted mode (default):
/// - Adjusts piece weights based on board danger level
/// - Guarantees at least one piece can be placed (prevents unfair instant losses)
/// - Uses smaller pieces when board is dangerous
/// - Handles edge cases with fallback mechanisms
/// 
/// Classic mode:
/// - 7-bag randomizer with weighted extended pieces
/// - Core tetrominoes always in each bag
/// - Extended pieces have 50% chance per bag
/// </summary>
public sealed class PieceBagGenerator
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
        PieceType.Rect2x3
    ];

    private readonly Random _random;
    private readonly int? _seed;
    private readonly bool _useWeightedGeneration;
    private readonly WeightedPieceSelector? _weightedSelector;
    private List<PieceType> _currentBag;
    private int _callCount; // Track RNG state for serialization

    /// <summary>
    /// Creates a new piece bag generator with optional seed for deterministic behavior.
    /// </summary>
    /// <param name="seed">Optional seed for reproducible results.</param>
    /// <param name="useWeightedGeneration">If true, uses board-aware weighted generation. Default is true.</param>
    public PieceBagGenerator(int? seed = null, bool useWeightedGeneration = true)
    {
        _seed = seed;
        _useWeightedGeneration = useWeightedGeneration;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _currentBag = [];
        _callCount = 0;

        if (_useWeightedGeneration)
        {
            _weightedSelector = new WeightedPieceSelector(seed);
        }

        // Only refill for classic mode - weighted mode needs board state
        if (!_useWeightedGeneration)
        {
            RefillBag();
        }
    }

    private PieceBagGenerator(List<PieceType> remainingPieces, int? seed, int callCount, bool useWeightedGeneration, string? weightedSelectorState)
    {
        _seed = seed;
        _useWeightedGeneration = useWeightedGeneration;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _callCount = 0;

        // Advance the random state to match the serialized state
        for (int i = 0; i < callCount; i++)
        {
            _random.Next();
        }
        _callCount = callCount;

        _currentBag = new List<PieceType>(remainingPieces);

        if (_useWeightedGeneration)
        {
            if (!string.IsNullOrEmpty(weightedSelectorState))
            {
                var (selector, _) = WeightedPieceSelector.FromState(weightedSelectorState);
                _weightedSelector = selector;
            }
            else
            {
                _weightedSelector = new WeightedPieceSelector(seed);
                // Advance weighted selector to same state (approximate fallback)
                // Ideally we always have state
            }
        }
    }

    /// <summary>
    /// Gets the next pieces using board-aware generation.
    /// This is the preferred method when you have access to the board state.
    /// </summary>
    /// <param name="count">Number of pieces to get.</param>
    /// <param name="board">The current game board for danger-aware generation.</param>
    /// <param name="linesCleared">Total lines cleared for progressive difficulty.</param>
    /// <returns>List of pieces.</returns>
    public List<Piece> GetNextPieces(int count, GameBoard board, int linesCleared = 0)
    {
        if (count <= 0)
            return [];

        if (_useWeightedGeneration && _weightedSelector != null)
        {
            return _weightedSelector.GeneratePieces(board, linesCleared, count);
        }

        // Fall back to classic generation
        return GetNextPieces(count);
    }

    /// <summary>
    /// Gets the next pieces, consuming them from the bag.
    /// Uses classic 7-bag generation (no board awareness).
    /// </summary>
    /// <param name="count">Number of pieces to get.</param>
    /// <returns>List of pieces.</returns>
    public List<Piece> GetNextPieces(int count)
    {
        if (count <= 0)
            return [];

        var result = new List<Piece>(count);

        for (int i = 0; i < count; i++)
        {
            if (_currentBag.Count == 0)
            {
                RefillBag();
            }

            var pieceType = _currentBag[0];
            _currentBag.RemoveAt(0);

            // Assign random rotation
            _callCount++;
            int rotation = _random.Next(4);
            result.Add(Piece.Create(pieceType, rotation));
        }

        return result;
    }

    /// <summary>
    /// Peeks at the next pieces without consuming them.
    /// </summary>
    /// <param name="count">Number of pieces to peek.</param>
    /// <returns>List of pieces.</returns>
    public List<Piece> PeekNextPieces(int count)
    {
        if (count <= 0)
            return [];

        // Ensure we have enough pieces to peek
        while (_currentBag.Count < count)
        {
            RefillBagPreserveExisting();
        }

        // Note: Peeking doesn't assign rotation permanently, just for display
        // This might be inconsistent if peek is used for logic
        // But for now, we just return random rotation (deterministic if seed is used, but call count matters)
        // Actually, peeking shouldn't advance RNG if possible, but here we need rotation.
        // Let's just return default rotation for peek to avoid side effects, or use a separate RNG?
        // For simplicity, let's return default rotation (0) for peek, as it's likely just for "next piece" preview
        // which might not show rotation.
        // OR, we can just return PieceType if the caller only needs type.
        // But the signature should match.
        
        return _currentBag.Take(count).Select(t => Piece.Create(t, 0)).ToList();
    }

    /// <summary>
    /// Serializes the current state for persistence.
    /// </summary>
    /// <returns>JSON string representing the state.</returns>
    public string SerializeState(List<Piece> currentPieces)
    {
        var state = new PieceBagState(
            new List<PieceType>(_currentBag),
            _seed,
            _callCount,
            _useWeightedGeneration,
            _weightedSelector?.SerializeState(currentPieces) 
        );

        return JsonSerializer.Serialize(state);
    }

    /// <summary>
    /// Creates a generator from a serialized state.
    /// </summary>
    /// <param name="stateJson">The JSON state string.</param>
    /// <returns>A new generator in the saved state.</returns>
    /// <exception cref="ArgumentException">If the state is null or empty.</exception>
    public static PieceBagGenerator FromState(string stateJson)
    {
        if (string.IsNullOrEmpty(stateJson))
        {
            throw new ArgumentException("State cannot be null or empty.", nameof(stateJson));
        }

        var state = JsonSerializer.Deserialize<PieceBagState>(stateJson)
            ?? throw new InvalidOperationException("Failed to deserialize state.");

        return new PieceBagGenerator(state.RemainingPieces, state.Seed, state.RandomState, state.UseWeightedGeneration, state.WeightedSelectorState);
    }

    private void RefillBag()
    {
        _currentBag = GenerateNewBag();
    }

    private void RefillBagPreserveExisting()
    {
        var newBag = GenerateNewBag();
        _currentBag.AddRange(newBag);
    }

    private List<PieceType> GenerateNewBag()
    {
        var bag = new List<PieceType>(CoreTetrominoes);

        // Add extended pieces at 50% rate
        foreach (var piece in ExtendedPieces)
        {
            _callCount++;
            if (_random.NextDouble() < 0.5)
            {
                bag.Add(piece);
            }
        }

        // Shuffle the bag using Fisher-Yates
        for (int i = bag.Count - 1; i > 0; i--)
        {
            _callCount++;
            int j = _random.Next(i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }

        return bag;
    }
}
