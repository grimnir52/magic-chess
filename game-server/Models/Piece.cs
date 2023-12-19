using game_server.Models.Constants;

namespace game_server.Models;

public class Piece(PieceType type, bool isFlipped, Color color)
{
    public PieceType? Type { get; set; } = type;
    public bool IsFlipped { get; set; } = isFlipped;
    public Color? Color { get; set; } = color;
}
