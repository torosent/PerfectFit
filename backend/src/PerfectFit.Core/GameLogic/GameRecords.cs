using PerfectFit.Core.GameLogic.Board;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.Core.GameLogic;

/// <summary>
/// Result of placing a piece on the board.
/// </summary>
/// <param name="Success">Whether the placement was successful.</param>
/// <param name="PointsEarned">Points earned from this placement.</param>
/// <param name="LinesCleared">Number of lines cleared.</param>
/// <param name="NewCombo">The new combo count after this placement.</param>
/// <param name="IsGameOver">Whether the game is over after this placement.</param>
/// <param name="ClearResult">Details about which lines were cleared, if any.</param>
public record PlacementResult(
    bool Success,
    int PointsEarned,
    int LinesCleared,
    int NewCombo,
    bool IsGameOver,
    ClearResult? ClearResult);

/// <summary>
/// Represents the complete state of a game for persistence.
/// </summary>
/// <param name="BoardGrid">The 10x10 board state.</param>
/// <param name="CurrentPieceTypes">The current 3 pieces available.</param>
/// <param name="PieceBagState">Serialized piece bag generator state.</param>
/// <param name="Score">Current score.</param>
/// <param name="Combo">Current combo count.</param>
/// <param name="TotalLinesCleared">Total lines cleared in the game.</param>
/// <param name="MaxCombo">Maximum combo achieved.</param>
public record GameState(
    string?[,] BoardGrid,
    List<PieceType> CurrentPieceTypes,
    string PieceBagState,
    int Score,
    int Combo,
    int TotalLinesCleared,
    int MaxCombo);
