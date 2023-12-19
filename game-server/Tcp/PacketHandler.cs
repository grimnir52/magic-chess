using game_server.Models;
using game_server.Repos;
using matchmaking_system.Handlers;

namespace game_server.Tcp;

public class PacketHandler(GameTableRepo tableService, ILogger<PacketHandler> logger)
{
    public async Task HandlePing(Packet packet)
    {
        logger.LogInformation("Recieved ping from matchmaking server");
        await PacketSender.SendPing();
    }

    public async Task HandleConnectRequest(Packet packet)
    {
        var gameId = packet.ReadString();
        logger.LogInformation("Recieved connect request from matchmaking server: {}", gameId);

        var game = await tableService.Get(gameId);
        if (game is null) await tableService.Add(new GameTable(gameId));

        var connectionString = $"http://localhost:5038/chess?token={gameId}";
        logger.LogInformation("Sending connection string: {}", connectionString);

        await PacketSender.SendConnectGranted(connectionString);
    }
}
