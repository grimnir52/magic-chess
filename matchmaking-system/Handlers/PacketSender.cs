using matchmaking_system.Servers;
using matchmaking_system.Utils;

namespace matchmaking_system.Handlers;

static class PacketSender
{
    public static async Task SendPing()
    {
        using var packet = new Packet();
        packet.Write((int)Packets.ping);
        packet.Write(true);

        await Server.gameServer.SendData(packet);
    }

    public static async Task SendConnectRequest(string gameId)
    {
        using var packet = new Packet();
        packet.Write((int)Packets.connectRequest);
        packet.Write(gameId);

        await Server.gameServer.SendData(packet);
    }

    public static async Task SendConnectGranted(string connectionString)
    {
        using var packet = new Packet();
        packet.Write(true);
        packet.Write(connectionString);

        await Server.frontEnd.SendData(packet);
    }
}
