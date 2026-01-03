namespace PerfectFit.Core.GameLogic.Generation;

using PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Defines weight configurations for piece selection based on board state.
/// Weights determine the relative probability of selecting each piece type.
/// </summary>
public sealed class PieceWeights
{
    /// <summary>
    /// Piece size categories for weight assignment.
    /// </summary>
    public enum PieceCategory
    {
        /// <summary>1 cell (Dot)</summary>
        Tiny,
        /// <summary>2 cells (Line2)</summary>
        Small,
        /// <summary>3 cells (Line3, Corner)</summary>
        Medium,
        /// <summary>4 cells (I, O, T, S, Z, J, L, Square2x2)</summary>
        Standard,
        /// <summary>5 cells (Line5, BigCorner)</summary>
        Large,
        /// <summary>9 cells (Square3x3)</summary>
        Huge
    }

    private static readonly Dictionary<PieceType, PieceCategory> PieceCategories = new()
    {
        [PieceType.Dot] = PieceCategory.Tiny,
        [PieceType.Line2] = PieceCategory.Small,
        [PieceType.Line3] = PieceCategory.Medium,
        [PieceType.Corner] = PieceCategory.Medium,
        [PieceType.I] = PieceCategory.Standard,
        [PieceType.O] = PieceCategory.Standard,
        [PieceType.T] = PieceCategory.Standard,
        [PieceType.S] = PieceCategory.Standard,
        [PieceType.Z] = PieceCategory.Standard,
        [PieceType.J] = PieceCategory.Standard,
        [PieceType.L] = PieceCategory.Standard,
        [PieceType.Square2x2] = PieceCategory.Standard,
        [PieceType.Line5] = PieceCategory.Large,
        [PieceType.BigCorner] = PieceCategory.Large,
        [PieceType.Square3x3] = PieceCategory.Huge
    };

    private static readonly Dictionary<PieceType, int> PieceCellCounts = new()
    {
        [PieceType.Dot] = 1,
        [PieceType.Line2] = 2,
        [PieceType.Line3] = 3,
        [PieceType.Corner] = 3,
        [PieceType.I] = 4,
        [PieceType.O] = 4,
        [PieceType.T] = 4,
        [PieceType.S] = 4,
        [PieceType.Z] = 4,
        [PieceType.J] = 4,
        [PieceType.L] = 4,
        [PieceType.Square2x2] = 4,
        [PieceType.Line5] = 5,
        [PieceType.BigCorner] = 5,
        [PieceType.Square3x3] = 9
    };

    /// <summary>
    /// Weights when board is safe (many empty cells, plenty of legal moves).
    /// Allows more challenging/large pieces.
    /// </summary>
    public static readonly IReadOnlyDictionary<PieceCategory, double> SafeWeights = new Dictionary<PieceCategory, double>
    {
        [PieceCategory.Tiny] = 0.5,      // Rare when safe
        [PieceCategory.Small] = 0.7,
        [PieceCategory.Medium] = 0.9,
        [PieceCategory.Standard] = 1.0,  // Base weight
        [PieceCategory.Large] = 0.8,
        [PieceCategory.Huge] = 0.4       // Less common but possible
    };

    /// <summary>
    /// Weights when board is dangerous (few empty cells, limited moves).
    /// Favors smaller, easier-to-place pieces.
    /// </summary>
    public static readonly IReadOnlyDictionary<PieceCategory, double> DangerWeights = new Dictionary<PieceCategory, double>
    {
        [PieceCategory.Tiny] = 2.0,      // Much more likely when in danger
        [PieceCategory.Small] = 1.8,
        [PieceCategory.Medium] = 1.5,
        [PieceCategory.Standard] = 1.0,  // Base weight
        [PieceCategory.Large] = 0.3,     // Rare when dangerous
        [PieceCategory.Huge] = 0.1       // Very rare when dangerous
    };

    /// <summary>
    /// Gets the category of a piece type.
    /// </summary>
    public static PieceCategory GetCategory(PieceType type) => PieceCategories[type];

    /// <summary>
    /// Gets the cell count of a piece type.
    /// </summary>
    public static int GetCellCount(PieceType type) => PieceCellCounts[type];

    /// <summary>
    /// Calculates the selection weight for a piece type given a danger level.
    /// Uses linear interpolation between safe and danger weights.
    /// </summary>
    /// <param name="type">The piece type to get weight for.</param>
    /// <param name="dangerLevel">Danger level from 0.0 (safe) to 1.0 (dangerous).</param>
    /// <returns>The interpolated weight for this piece type.</returns>
    public static double GetWeight(PieceType type, double dangerLevel)
    {
        var category = GetCategory(type);
        var safeWeight = SafeWeights[category];
        var dangerWeight = DangerWeights[category];

        // Clamp danger level
        dangerLevel = Math.Clamp(dangerLevel, 0.0, 1.0);

        // Linear interpolation: lerp(safe, danger, dangerLevel)
        return safeWeight + (dangerWeight - safeWeight) * dangerLevel;
    }

    /// <summary>
    /// Gets all weights for a given danger level.
    /// </summary>
    /// <param name="dangerLevel">Danger level from 0.0 to 1.0.</param>
    /// <returns>Dictionary of piece types to their weights.</returns>
    public static Dictionary<PieceType, double> GetAllWeights(double dangerLevel)
    {
        var weights = new Dictionary<PieceType, double>();
        foreach (var type in Enum.GetValues<PieceType>())
        {
            weights[type] = GetWeight(type, dangerLevel);
        }
        return weights;
    }

    /// <summary>
    /// All available piece types.
    /// </summary>
    public static IReadOnlyList<PieceType> AllPieceTypes { get; } = Enum.GetValues<PieceType>().ToList();
}
