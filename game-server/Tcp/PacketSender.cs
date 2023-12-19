using game_server.Tcp;

namespace matchmaking_system.Handlers;

static class PacketSender
{
    public static async Task SendPing()
    {
        using var packet = new Packet();
        packet.Write((int)Packets.ping);
        packet.Write(true);

        await Client.SendData(packet);
    }

    public static async Task SendConnectGranted(string connectionString)
    {
        using var packet = new Packet();
        packet.Write((int)Packets.connectGranted);
        packet.Write(connectionString);

        await Client.SendData(packet);
    }

    public static async Task SendGameFinished(string gameId)
    {
        using var packet = new Packet();
        packet.Write((int)Packets.gameFinished);
        packet.Write(gameId);

        await Client.SendData(packet);
    }
}
