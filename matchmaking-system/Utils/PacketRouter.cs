using matchmaking_system.Handlers;

namespace matchmaking_system.Utils;

// used to route packets to packet handler depending on thier type using a delegate
static class PacketRouter
{
    private delegate Task Handler(Packet packet);
    private static Dictionary<Packets, Handler> Handlers { get; set; } = [];

    static PacketRouter()
    {
        Handlers[Packets.ping] = PacketHandler.HandlePing;
        Handlers[Packets.connectRequest] = PacketHandler.HandleConnectRequest;
        Handlers[Packets.connectGranted] = PacketHandler.HandleConnectGranted;
        Handlers[Packets.gameFinished] = PacketHandler.HandleGameFinished;
    }

    public static async Task Route(byte[] data)
    {
        using var packet = new Packet(data);
        var type = (Packets)packet.ReadInt();

        await Handlers[type]?.Invoke(packet);
    }
}
