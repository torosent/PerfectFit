using FluentAssertions;
using PerfectFit.Core.GameLogic.Pieces;

namespace PerfectFit.UnitTests.GameLogic;

public class PieceTests
{
    [Fact]
    public void Create_Rect2x3_Rotation0_HasHotPinkColor()
    {
        var piece = Piece.Create(PieceType.Rect2x3, 0);
        piece.Color.Should().Be("#FF69B4"); // Hot Pink
    }

    [Fact]
    public void Create_Rect2x3_Rotation1_HasRoyalBlueColor()
    {
        var piece = Piece.Create(PieceType.Rect2x3, 1);
        piece.Color.Should().Be("#4169E1"); // Royal Blue
    }

    [Fact]
    public void Create_Rect2x3_Rotation2_HasHotPinkColor()
    {
        var piece = Piece.Create(PieceType.Rect2x3, 2);
        piece.Color.Should().Be("#FF69B4"); // Hot Pink
    }

    [Fact]
    public void Create_Rect2x3_Rotation3_HasRoyalBlueColor()
    {
        var piece = Piece.Create(PieceType.Rect2x3, 3);
        piece.Color.Should().Be("#4169E1"); // Royal Blue
    }

    [Fact]
    public void Rotate_Rect2x3_ChangesColor()
    {
        var piece = Piece.Create(PieceType.Rect2x3, 0);
        piece.Color.Should().Be("#FF69B4");

        var rotated = piece.Rotate();
        rotated.Rotation.Should().Be(1);
        rotated.Color.Should().Be("#4169E1");

        var rotated2 = rotated.Rotate();
        rotated2.Rotation.Should().Be(2);
        rotated2.Color.Should().Be("#FF69B4");
    }

    [Fact]
    public void Create_OtherPiece_RotationDoesNotChangeColor()
    {
        var piece0 = Piece.Create(PieceType.T, 0);
        var piece1 = Piece.Create(PieceType.T, 1);

        piece0.Color.Should().Be(piece1.Color);
    }
}
