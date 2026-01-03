namespace PerfectFit.Core.GameLogic.Generation;

using System.Text.Json;
using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// State for serialization of the weighted piece selector.
/// </summary>
internal record WeightedSelectorState(
    List<PieceType> CurrentPieces,
    int? Seed,
    int RandomCallCount);

/// <summary>
/// Generates pieces using weighted randomness that adapts to board state.
/// 
/// Key features:
/// - Danger-based weight adjustment: safer boards allow larger pieces, dangerous boards favor small pieces
/// - Solvability guarantee: ensures at least one piece can always be placed
/// - Fallback mechanisms: handles edge cases where no good pieces are available
/// - Max retry limits: prevents infinite loops
/// </summary>
public sealed class WeightedPieceSelector
{
    private const int MaxGenerationRetries = 50;
    private const int PieceHandSize = 3;

    private readonly Random _random;
    private readonly int? _seed;
    private int _randomCallCount;

    /// <summary>
    /// Creates a new weighted piece selector with optional seed for deterministic behavior.
    /// </summary>
    /// <param name="seed">Optional seed for reproducible results.</param>
    public WeightedPieceSelector(int? seed = null)
    {
        _seed = seed;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _randomCallCount = 0;
    }

    private WeightedPieceSelector(int? seed, int randomCallCount)
    {
        _seed = seed;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();

        // Advance the random state to match the serialized state
        for (int i = 0; i < randomCallCount; i++)
        {
            _random.Next();
        }
        _randomCallCount = randomCallCount;
    }

    /// <summary>
    /// Generates a set of pieces appropriate for the current board state.
    /// Guarantees that at least one piece can be placed.
    /// </summary>
    /// <param name="board">The current game board.</param>
    /// <param name="count">Number of pieces to generate (default 3).</param>
    /// <returns>List of generated piece types.</returns>
    public List<PieceType> GeneratePieces(GameBoard board, int count = PieceHandSize)
    {
        if (count <= 0)
            return [];

        var analysis = BoardAnalyzer.Analyze(board);
        var dangerLevel = analysis.DangerLevel;

        // Get weights for current danger level
        var weights = PieceWeights.GetAllWeights(dangerLevel);

        var selectedPieces = new List<PieceType>(count);
        int retries = 0;

        while (selectedPieces.Count < count && retries < MaxGenerationRetries)
        {
            // Generate candidate set
            var candidates = SelectPiecesWeighted(weights, count - selectedPieces.Count);

            // Combine with already selected pieces
            var testSet = new List<PieceType>(selectedPieces);
            testSet.AddRange(candidates);

            // Check solvability
            var result = SolvabilityChecker.CheckSolvability(board, testSet);

            if (result.AtLeastOneFits)
            {
                // Accept these candidates
                selectedPieces.AddRange(candidates);

                // If not all fit, we might want to try regenerating, but accept for now
                // to avoid endless loops. The game will end if player can't place.
                if (!result.IsSolvable && dangerLevel > 0.8)
                {
                    // In high danger, replace some pieces with guaranteed-fitting ones
                    selectedPieces = EnsureAtLeastOneFits(board, selectedPieces, weights);
                }
            }
            else
            {
                // None fit - increase danger level to prefer smaller pieces
                dangerLevel = Math.Min(1.0, dangerLevel + 0.2);
                weights = PieceWeights.GetAllWeights(dangerLevel);
                retries++;
            }
        }

        // Final fallback: If still nothing fits, use emergency generation
        if (selectedPieces.Count < count)
        {
            var emergency = EmergencyGenerate(board, count - selectedPieces.Count);
            selectedPieces.AddRange(emergency);
        }

        // Ensure at least one piece fits (absolute guarantee)
        if (!SolvabilityChecker.AtLeastOneFits(board, selectedPieces))
        {
            selectedPieces = ForceOneFittingPiece(board, selectedPieces);
        }

        return selectedPieces;
    }

    /// <summary>
    /// Selects pieces using weighted random selection.
    /// </summary>
    private List<PieceType> SelectPiecesWeighted(Dictionary<PieceType, double> weights, int count)
    {
        var result = new List<PieceType>(count);
        var pieceTypes = PieceWeights.AllPieceTypes;
        var totalWeight = weights.Values.Sum();

        for (int i = 0; i < count; i++)
        {
            var piece = SelectOneWeighted(pieceTypes, weights, totalWeight);
            result.Add(piece);
        }

        return result;
    }

    /// <summary>
    /// Selects a single piece using weighted random.
    /// </summary>
    private PieceType SelectOneWeighted(IReadOnlyList<PieceType> pieceTypes, Dictionary<PieceType, double> weights, double totalWeight)
    {
        _randomCallCount++;
        double roll = _random.NextDouble() * totalWeight;

        double cumulative = 0;
        foreach (var pieceType in pieceTypes)
        {
            cumulative += weights[pieceType];
            if (roll <= cumulative)
            {
                return pieceType;
            }
        }

        // Fallback (should not reach here)
        return pieceTypes[^1];
    }

    /// <summary>
    /// Ensures at least one piece in the set can fit on the board.
    /// Replaces non-fitting pieces with smaller alternatives.
    /// </summary>
    private List<PieceType> EnsureAtLeastOneFits(GameBoard board, List<PieceType> pieces, Dictionary<PieceType, double> weights)
    {
        var fittingPieces = SolvabilityChecker.GetFittingPieces(board, pieces);

        if (fittingPieces.Count > 0)
        {
            return pieces; // Already has fitting pieces
        }

        // Replace the largest piece with a smaller alternative
        var result = new List<PieceType>(pieces);

        // Sort by cell count descending to replace largest first
        var sortedBySize = result
            .Select((p, i) => (Type: p, Index: i, Size: PieceWeights.GetCellCount(p)))
            .OrderByDescending(x => x.Size)
            .ToList();

        // Try replacing pieces from largest to smallest until one fits
        foreach (var item in sortedBySize)
        {
            // Try each piece type from smallest to largest
            var smallPieces = PieceWeights.AllPieceTypes
                .OrderBy(p => PieceWeights.GetCellCount(p))
                .ToList();

            foreach (var smallPiece in smallPieces)
            {
                var piece = Piece.Create(smallPiece);
                if (board.CanPlacePieceAnywhere(piece))
                {
                    result[item.Index] = smallPiece;
                    return result;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Emergency generation when normal generation fails.
    /// Uses only the smallest pieces.
    /// </summary>
    private List<PieceType> EmergencyGenerate(GameBoard board, int count)
    {
        var result = new List<PieceType>(count);

        // Priority order: smallest to largest
        var priorityPieces = new[]
        {
            PieceType.Dot,
            PieceType.Line2,
            PieceType.Line3,
            PieceType.Corner,
            PieceType.Square2x2,
            PieceType.T,
            PieceType.I,
            PieceType.O,
            PieceType.S,
            PieceType.Z,
            PieceType.J,
            PieceType.L
        };

        // Find pieces that can actually fit
        var fittingPieces = new List<PieceType>();
        foreach (var pieceType in priorityPieces)
        {
            var piece = Piece.Create(pieceType);
            if (board.CanPlacePieceAnywhere(piece))
            {
                fittingPieces.Add(pieceType);
            }
        }

        // If some pieces fit, use them
        if (fittingPieces.Count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                _randomCallCount++;
                var index = _random.Next(Math.Min(fittingPieces.Count, 3)); // Prefer smallest
                result.Add(fittingPieces[index]);
            }
        }
        else
        {
            // Nothing fits - game will end. Generate small pieces anyway.
            for (int i = 0; i < count; i++)
            {
                result.Add(PieceType.Dot);
            }
        }

        return result;
    }

    /// <summary>
    /// Forces at least one piece in the set to be placeable.
    /// Used as absolute last resort.
    /// </summary>
    private List<PieceType> ForceOneFittingPiece(GameBoard board, List<PieceType> pieces)
    {
        var result = new List<PieceType>(pieces);

        // Find smallest fitting piece
        foreach (var pieceType in PieceWeights.AllPieceTypes.OrderBy(p => PieceWeights.GetCellCount(p)))
        {
            var piece = Piece.Create(pieceType);
            if (board.CanPlacePieceAnywhere(piece))
            {
                // Replace the first piece with this one
                if (result.Count > 0)
                {
                    result[0] = pieceType;
                }
                else
                {
                    result.Add(pieceType);
                }
                return result;
            }
        }

        // If absolutely nothing fits, return original (game will end)
        return result;
    }

    /// <summary>
    /// Serializes the current state for persistence.
    /// </summary>
    public string SerializeState(List<PieceType> currentPieces)
    {
        var state = new WeightedSelectorState(
            CurrentPieces: new List<PieceType>(currentPieces),
            Seed: _seed,
            RandomCallCount: _randomCallCount
        );

        return JsonSerializer.Serialize(state);
    }

    /// <summary>
    /// Creates a selector from a serialized state.
    /// </summary>
    public static (WeightedPieceSelector Selector, List<PieceType> Pieces) FromState(string stateJson)
    {
        if (string.IsNullOrEmpty(stateJson))
        {
            throw new ArgumentException("State cannot be null or empty.", nameof(stateJson));
        }

        var state = JsonSerializer.Deserialize<WeightedSelectorState>(stateJson)
            ?? throw new InvalidOperationException("Failed to deserialize state.");

        var selector = new WeightedPieceSelector(state.Seed, state.RandomCallCount);
        return (selector, new List<PieceType>(state.CurrentPieces));
    }
}
