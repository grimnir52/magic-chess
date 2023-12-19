using game_server.Models.Constants;
using MongoDB.Bson.Serialization.Attributes;

namespace game_server.Models;

public class GameTable
{
    [BsonId]
    public string Id { get; set; }

    public Dictionary<string, Player> Players { get; set; } = [];
    public State State { get; set; } = State.pending;
    public int MaxPlayers { get; set; } = 2;
    public Board Board { get; set; }
    public Color Turn { get; set; } = Color.red;

    public GameTable(string id)
    {
        Id = id;
        Board = new Board();
    }

    public GameTable(string id, Board board)
    {
        Id = id;
        Board = board;
    }
}