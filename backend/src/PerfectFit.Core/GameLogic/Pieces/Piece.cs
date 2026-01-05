namespace PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Represents a game piece with its type, shape, and color.
/// </summary>
/// <param name="Type">The type of piece.</param>
/// <param name="Shape">A 2D boolean array representing the shape where true indicates a filled cell.</param>
/// <param name="Color">The hex color code for this piece.</param>
/// <param name="Rotation">The rotation of the piece (0=0, 1=90, 2=180, 3=270 degrees clockwise).</param>
public sealed record Piece(PieceType Type, bool[,] Shape, string Color, int Rotation = 0)
{
    /// <summary>
    /// Factory method to create a piece of the specified type with predefined shape and color.
    /// </summary>
    /// <param name="type">The type of piece to create.</param>
    /// <param name="rotation">The rotation (0-3).</param>
    /// <returns>A new Piece instance.</returns>
    public static Piece Create(PieceType type, int rotation = 0)
    {
        var baseShape = PieceDefinitions.GetShape(type);
        var rotatedShape = RotateShape(baseShape, rotation);
        
        return new Piece(
            type,
            rotatedShape,
            PieceDefinitions.GetColor(type, rotation),
            rotation
        );
    }

    /// <summary>
    /// Creates a new piece with the shape rotated 90 degrees clockwise.
    /// </summary>
    /// <returns>A new rotated Piece instance.</returns>
    public Piece Rotate()
    {
        var newRotation = (Rotation + 1) % 4;
        var newShape = RotateShape(Shape, 1);
        return new Piece(Type, newShape, PieceDefinitions.GetColor(Type, newRotation), newRotation);
    }

    private static bool[,] RotateShape(bool[,] shape, int rotations)
    {
        rotations %= 4;
        if (rotations == 0) return shape;

        var currentShape = shape;
        for (int i = 0; i < rotations; i++)
        {
            currentShape = Rotate90(currentShape);
        }
        return currentShape;
    }

    private static bool[,] Rotate90(bool[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        var newMatrix = new bool[cols, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                newMatrix[c, rows - 1 - r] = matrix[r, c];
            }
        }

        return newMatrix;
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
