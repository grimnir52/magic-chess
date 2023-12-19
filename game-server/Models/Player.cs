using game_server.Models.Constants;

namespace game_server.Models;

public class Player(Color color, int piecesLeft)
{
    public Color Color { get; set; } = color;
    public int PiecesLeft { get; set; } = piecesLeft;
    public bool Ready { get; set; } = false;
}
