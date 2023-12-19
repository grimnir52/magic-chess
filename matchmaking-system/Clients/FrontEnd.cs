using static System.Console;
using System.Net;
using System.Net.Sockets;
using matchmaking_system.Utils;

namespace matchmaking_system.Clients;

// ucp client wrapper class for the front end asp.net server
public class FrontEnd
{
    private UdpClient client;
    private IPEndPoint endPoint;

    //handels connection to the udp endpoint
    public async Task HandleConnect(UdpClient client, UdpReceiveResult result)
    {
        var data = result.Buffer;
        if (data.Length < 4) return;

        var endPoint = result.RemoteEndPoint;
        if (this.endPoint != null && this.endPoint != endPoint) return;

        this.endPoint = endPoint;
        this.client = client;


        try { _ = PacketRouter.Route(data); } //routes the first packet to the packet handler
        catch (Exception e)
        {
            Disconnect();
            WriteLine($"Error receiving first piece of data from front end: {e}");
        }

        while (true)
        {
            // continue recieving packets after the first one, which is considered as a connection 
            // since udp is connection-less
            try
            {
                var receiveResult = await this.client.ReceiveAsync();
                if (receiveResult.RemoteEndPoint.ToString() != this.endPoint.ToString()) continue;

                var receivedData = receiveResult.Buffer;
                if (receivedData.Length < 4) return;

                _ = PacketRouter.Route(receivedData);
            }
            catch (Exception e)
            {
                Disconnect();
                WriteLine($"Error receiving data from front end: {e}");
            }
        }
    }

    public async Task SendData(Packet packet)
    {
        try
        {
            if (client is not null)
            {
                await client.SendAsync(packet.ToArray(), endPoint);
            }
        }
        catch (Exception e)
        {
            Disconnect();
            WriteLine($"Error sending data to front end: {e}");
        }
    }

    public bool IsConnected() => endPoint != null;

    private void Disconnect() => endPoint = null;
}
