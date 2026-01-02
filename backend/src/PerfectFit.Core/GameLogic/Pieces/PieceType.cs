namespace PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Defines all available piece types in the PerfectFit game.
/// </summary>
public enum PieceType
{
    // Tetrominoes (4 cells each)
    I,
    O,
    T,
    S,
    Z,
    J,
    L,

    // Lines
    Dot,
    Line2,
    Line3,
    Line5,

    // Shapes
    Corner,
    BigCorner,
    Square2x2,
    Square3x3
}
