using System.Net;
using System.Net.Sockets;

namespace front_end.Udp;

public class PlayerClient
{
    public readonly string Ip = "127.0.0.1";
    public readonly int Port = 13;

    public UdpClient client;
    public IPEndPoint endPoint;

    public PlayerClient()
    {
        endPoint = new IPEndPoint(IPAddress.Parse(Ip), Port);
        client = new UdpClient(3500);
        client.Connect(endPoint);
    }

    public async Task<string> SendConnectRequest()
    {
        using (var packet = new Packet())
        {
            packet.Write((int)Packets.connectRequest);
            await client.SendAsync(packet.ToArray());
        }

        var connected = await client.ReceiveAsync();
        using var recievedPacket = new Packet(connected.Buffer);

        var isConnected = recievedPacket.ReadBool();
        var connectionString = recievedPacket.ReadString();

        return connectionString;
    }
}
