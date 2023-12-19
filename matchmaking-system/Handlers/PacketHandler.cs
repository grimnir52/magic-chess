using static System.Console;
using static System.Guid;
using matchmaking_system.Utils;
using matchmaking_system.Servers;

namespace matchmaking_system.Handlers;

// handles packets by thier type
static class PacketHandler
{
    public static Task HandlePing(Packet packet)
    {
        WriteLine("Recieved ping");
        return Task.CompletedTask;
    }

    public static async Task HandleConnectRequest(Packet packet)
    {
        WriteLine("Recieved connect request");
        if (Server.gameServer == null || !Server.gameServer.IsConnected()) return;
        var game = Server.games.FirstOrDefault(g => g.Players < 2) ?? new(NewGuid().ToString());
        if (!Server.games.Contains(game)) Server.games.Add(game);
        else game.Players++;


        Server.games.ForEach(g => WriteLine($"game: {game.Id} players: {game.Players}"));

        await PacketSender.SendConnectRequest(game.Id);
    }

    public static async Task HandleConnectGranted(Packet packet)
    {
        WriteLine("Recieved connect granted");
        var connectionString = packet.ReadString();
        WriteLine($"connection granted with: {connectionString}");

        await PacketSender.SendConnectGranted(connectionString);
    }

    public static Task HandleGameFinished(Packet packet)
    {
        WriteLine("Recieved Game Finished");
        var gameId = packet.ReadString();
        WriteLine($"Game: {gameId} has finished");

        var game = Server.games.FirstOrDefault(g => g.Id == gameId);
        if (game is not null) Server.games.Remove(game);

        return Task.CompletedTask;
    }
}
