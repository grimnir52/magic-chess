namespace game_server.Tcp;

public class PacketRouter
{
    private delegate Task Handler(Packet packet);
    private Dictionary<Packets, Handler> Handlers { get; set; } = [];

    public PacketRouter(PacketHandler handler)
    {
        Handlers[Packets.ping] = handler.HandlePing;
        Handlers[Packets.connectRequest] = handler.HandleConnectRequest;
    }

    public async Task Route(byte[] data)
    {
        using var packet = new Packet(data);
        var type = (Packets)packet.ReadInt();

        await Handlers[type]?.Invoke(packet);
    }
}
