namespace PerfectFit.Core.GameLogic.Pieces;

/// <summary>
/// Static class containing all piece shape and color definitions.
/// </summary>
public static class PieceDefinitions
{
    private static readonly Dictionary<PieceType, bool[,]> Shapes = new()
    {
        // Tetrominoes
        [PieceType.I] = new bool[,] { { true, true, true, true } },  // 1x4 horizontal
        [PieceType.O] = new bool[,] { { true, true }, { true, true } },  // 2x2
        [PieceType.T] = new bool[,] { { true, true, true }, { false, true, false } },  // 2x3
        [PieceType.S] = new bool[,] { { false, true, true }, { true, true, false } },  // 2x3
        [PieceType.Z] = new bool[,] { { true, true, false }, { false, true, true } },  // 2x3
        [PieceType.J] = new bool[,] { { true, false }, { true, false }, { true, true } },  // 3x2
        [PieceType.L] = new bool[,] { { false, true }, { false, true }, { true, true } },  // 3x2

        // Lines
        [PieceType.Dot] = new bool[,] { { true } },  // 1x1
        [PieceType.Line2] = new bool[,] { { true, true } },  // 1x2
        [PieceType.Line3] = new bool[,] { { true, true, true } },  // 1x3
        [PieceType.Line5] = new bool[,] { { true, true, true, true, true } },  // 1x5

        // Shapes
        [PieceType.Corner] = new bool[,] { { true, true }, { true, false } },  // 2x2, 3 cells
        [PieceType.BigCorner] = new bool[,]
        {
            { true, true, true },
            { true, false, false },
            { true, false, false }
        },  // 3x3, 5 cells
        [PieceType.Square2x2] = new bool[,] { { true, true }, { true, true } },  // 2x2
        [PieceType.Square3x3] = new bool[,]
        {
            { true, true, true },
            { true, true, true },
            { true, true, true }
        },  // 3x3, 9 cells
        [PieceType.Rect2x3] = new bool[,]
        {
            { true, true, true },
            { true, true, true }
        }  // 2x3, 6 cells
    };

    private static readonly Dictionary<PieceType, string> Colors = new()
    {
        // Tetrominoes
        [PieceType.I] = "#00FFFF",  // Cyan
        [PieceType.O] = "#FFFF00",  // Yellow
        [PieceType.T] = "#800080",  // Purple
        [PieceType.S] = "#00FF00",  // Green
        [PieceType.Z] = "#FF0000",  // Red
        [PieceType.J] = "#0000FF",  // Blue
        [PieceType.L] = "#FFA500",  // Orange

        // Lines
        [PieceType.Dot] = "#808080",  // Gray
        [PieceType.Line2] = "#FFB6C1",  // Light Pink
        [PieceType.Line3] = "#90EE90",  // Light Green
        [PieceType.Line5] = "#87CEEB",  // Sky Blue

        // Shapes
        [PieceType.Corner] = "#DDA0DD",  // Plum
        [PieceType.BigCorner] = "#F0E68C",  // Khaki
        [PieceType.Square2x2] = "#CD853F",  // Peru
        [PieceType.Square3x3] = "#8B4513",  // Saddle Brown
        [PieceType.Rect2x3] = "#FF69B4"   // Hot Pink
    };

    /// <summary>
    /// Gets the shape definition for a piece type.
    /// </summary>
    /// <param name="type">The piece type.</param>
    /// <returns>A 2D boolean array representing the piece shape.</returns>
    public static bool[,] GetShape(PieceType type)
    {
        return Shapes[type];
    }

    /// <summary>
    /// Gets the color for a piece type, accounting for rotation.
    /// </summary>
    /// <param name="type">The piece type.</param>
    /// <param name="rotation">The rotation (0-3).</param>
    /// <returns>A hex color string.</returns>
    public static string GetColor(PieceType type, int rotation = 0)
    {
        // Special case: Rect2x3 changes color when rotated to vertical (90 or 270 degrees)
        // to mimic the old Rect3x2 piece.
        if (type == PieceType.Rect2x3 && (rotation == 1 || rotation == 3))
        {
            return "#4169E1"; // Royal Blue (formerly Rect3x2)
        }

        return Colors[type];
    }
}
