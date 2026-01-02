namespace PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Represents a game piece with its type, shape, and color.
/// </summary>
/// <param name="Type">The type of piece.</param>
/// <param name="Shape">A 2D boolean array representing the shape where true indicates a filled cell.</param>
/// <param name="Color">The hex color code for this piece.</param>
public sealed record Piece(PieceType Type, bool[,] Shape, string Color)
{
    /// <summary>
    /// Factory method to create a piece of the specified type with predefined shape and color.
    /// </summary>
    /// <param name="type">The type of piece to create.</param>
    /// <returns>A new Piece instance.</returns>
    public static Piece Create(PieceType type)
    {
        return new Piece(
            type,
            PieceDefinitions.GetShape(type),
            PieceDefinitions.GetColor(type)
        );
    }

    /// <summary>
    /// Gets the number of rows in the piece shape.
    /// </summary>
    public int Rows => Shape.GetLength(0);

    /// <summary>
    /// Gets the number of columns in the piece shape.
    /// </summary>
    public int Columns => Shape.GetLength(1);
}
