namespace matchmaking_system.Models;

public class Game(string id)
{
    public string Id { get; set; } = id;
    public int Players { get; set; } = 1;
}
