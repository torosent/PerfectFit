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
    int RandomCallCount,
    List<PieceType> RecentHistory);

/// <summary>
/// Generates pieces using weighted randomness that adapts to board state.
/// 
/// Key features:
/// - Danger-based weight adjustment: safer boards allow larger pieces, dangerous boards favor small pieces
/// - Solvability guarantee: ensures at least one piece can always be placed
/// - Fallback mechanisms: handles edge cases where no good pieces are available
/// - Max retry limits: prevents infinite loops
/// - Progressive difficulty: favors larger pieces as score increases
/// - Rescue bias: guarantees small pieces when board is dangerous
/// - Repetition control: reduces probability of recently seen pieces
/// </summary>
public sealed class WeightedPieceSelector
{
    private const int MaxGenerationRetries = 50;
    private const int PieceHandSize = 3;
    private const int HistorySize = 5;

    private readonly Random _random;
    private readonly int? _seed;
    private int _randomCallCount;
    private readonly Queue<PieceType> _recentHistory;

    /// <summary>
    /// Creates a new weighted piece selector with optional seed for deterministic behavior.
    /// </summary>
    /// <param name="seed">Optional seed for reproducible results.</param>
    public WeightedPieceSelector(int? seed = null)
    {
        _seed = seed;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        _randomCallCount = 0;
        _recentHistory = new Queue<PieceType>(HistorySize);
    }

    private WeightedPieceSelector(int? seed, int randomCallCount, List<PieceType> history)
    {
        _seed = seed;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();

        // Advance the random state to match the serialized state
        for (int i = 0; i < randomCallCount; i++)
        {
            _random.Next();
        }
        _randomCallCount = randomCallCount;
        _recentHistory = new Queue<PieceType>(history);
    }

    /// <summary>
    /// Generates a set of pieces appropriate for the current board state.
    /// Guarantees that at least one piece can be placed.
    /// </summary>
    /// <param name="board">The current game board.</param>
    /// <param name="linesCleared">Total lines cleared in the game (for progressive difficulty).</param>
    /// <param name="count">Number of pieces to generate (default 3).</param>
    /// <returns>List of generated piece types.</returns>
    public List<PieceType> GeneratePieces(GameBoard board, int linesCleared = 0, int count = PieceHandSize)
    {
        if (count <= 0)
            return [];

        var analysis = BoardAnalyzer.Analyze(board);
        var dangerLevel = analysis.DangerLevel;

        // Get base weights for current danger level
        var weights = PieceWeights.GetAllWeights(dangerLevel);

        // Apply progressive difficulty (favor larger pieces as score increases)
        ApplyProgressiveDifficulty(weights, linesCleared);

        // Apply repetition control (reduce weight of recent pieces)
        ApplyRepetitionControl(weights);

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
                // Re-apply modifiers to new weights
                ApplyProgressiveDifficulty(weights, linesCleared);
                ApplyRepetitionControl(weights);

                retries++;
            }
        }

        // Final fallback: If still nothing fits, use emergency generation
        if (selectedPieces.Count < count)
        {
            var emergency = EmergencyGenerate(board, count - selectedPieces.Count);
            selectedPieces.AddRange(emergency);
        }

        // Rescue Bias: If danger is high, guarantee at least one small piece
        if (analysis.DangerLevel > 0.7)
        {
            EnsureRescuePiece(selectedPieces);
        }

        // Ensure at least one piece fits (absolute guarantee)
        if (!SolvabilityChecker.AtLeastOneFits(board, selectedPieces))
        {
            selectedPieces = ForceOneFittingPiece(board, selectedPieces);
        }

        // Update history
        foreach (var piece in selectedPieces)
        {
            if (_recentHistory.Count >= HistorySize)
            {
                _recentHistory.Dequeue();
            }
            _recentHistory.Enqueue(piece);
        }

        return selectedPieces;
    }

    private void ApplyProgressiveDifficulty(Dictionary<PieceType, double> weights, int linesCleared)
    {
        // Progression factor: 0.0 to 1.0 over 100 lines
        double progression = Math.Min(1.0, linesCleared / 100.0);

        if (progression <= 0) return;

        foreach (var type in weights.Keys.ToList())
        {
            var category = PieceWeights.GetCategory(type);
            double multiplier = 1.0;

            // Increase weight for larger pieces based on progression
            switch (category)
            {
                case PieceWeights.PieceCategory.Large:
                case PieceWeights.PieceCategory.Heavy:
                    multiplier = 1.0 + (progression * 0.5); // Up to 1.5x
                    break;
                case PieceWeights.PieceCategory.Huge:
                    multiplier = 1.0 + (progression * 0.8); // Up to 1.8x
                    break;
                case PieceWeights.PieceCategory.Tiny:
                case PieceWeights.PieceCategory.Small:
                    multiplier = 1.0 - (progression * 0.3); // Down to 0.7x
                    break;
            }

            weights[type] *= multiplier;
        }
    }

    private void ApplyRepetitionControl(Dictionary<PieceType, double> weights)
    {
        foreach (var piece in _recentHistory)
        {
            if (weights.ContainsKey(piece))
            {
                // Reduce weight by 50% for each occurrence in recent history
                weights[piece] *= 0.5;
            }
        }
    }

    private void EnsureRescuePiece(List<PieceType> pieces)
    {
        // Check if we already have a small piece
        if (pieces.Any(p => PieceWeights.GetCellCount(p) <= 2))
        {
            return;
        }

        // Replace the largest piece with a small one (Dot or Line2)
        var largestIndex = -1;
        var maxCells = -1;

        for (int i = 0; i < pieces.Count; i++)
        {
            var cells = PieceWeights.GetCellCount(pieces[i]);
            if (cells > maxCells)
            {
                maxCells = cells;
                largestIndex = i;
            }
        }

        if (largestIndex != -1)
        {
            // 50% chance of Dot, 50% chance of Line2
            _randomCallCount++;
            pieces[largestIndex] = _random.NextDouble() < 0.5 ? PieceType.Dot : PieceType.Line2;
        }
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
            RandomCallCount: _randomCallCount,
            RecentHistory: _recentHistory.ToList()
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

        var selector = new WeightedPieceSelector(state.Seed, state.RandomCallCount, state.RecentHistory);
        return (selector, new List<PieceType>(state.CurrentPieces));
    }
}
