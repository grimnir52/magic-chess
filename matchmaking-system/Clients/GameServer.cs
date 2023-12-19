using static System.Console;
using System.Net.Sockets;
using matchmaking_system.Utils;
using matchmaking_system.Servers;

namespace matchmaking_system.Clients;

// tcp client wrapper class for the game server
public class GameServer
{
    private TcpClient tcpClient;
    private Packet receivedData;
    private NetworkStream stream;
    private byte[] receiveBuffer;

    // timer used for pinging the on an interval
    private static Timer pingTimer;

    private readonly int bufferSize = 4096;

    public async Task HandleConnect(TcpClient tcpClient)
    {
        receivedData = new();
        this.tcpClient = tcpClient;
        stream = tcpClient.GetStream();
        receiveBuffer = new byte[bufferSize];

        _ = StartPinging(); // task is not awaited on purpose to allow it to run in the background 
        await RecieveData();
    }

    private async Task RecieveData()
    {
        try
        {
            while (true)
            {
                var length = await stream.ReadAsync(receiveBuffer);
                if (length <= 0)
                {
                    Disconnect();
                    WriteLine("Game server disconnected");
                }

                var data = new byte[length];
                Array.Copy(receiveBuffer, data, length);

                _ = HandleData(data);
            }
        }
        catch (Exception e)
        {
            Disconnect();
            WriteLine($"Error recieving game server data: {e}");
        }
    }

    public async Task SendData(Packet packet)
    {
        try
        {
            if (tcpClient is not null)
            {
                packet.WriteLength();
                await stream.WriteAsync(packet.ToArray());
            }
        }
        catch (Exception e)
        {
            Disconnect();
            WriteLine($"Error sending data to game server: {e}");
        }
    }

    public bool IsConnected() => tcpClient != null && tcpClient.Connected;

    private async Task HandleData(byte[] data)
    {
        try
        {
            var packetLength = 0;

            receivedData.SetBytes(data);

            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();
                if (packetLength <= 0) receivedData.Reset(true);
            }

            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                var bytes = receivedData.ReadBytes(packetLength);

                await PacketRouter.Route(bytes);

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();
                    if (packetLength <= 0) receivedData.Reset(true);
                }
                else packetLength = 0;
            }

            if (packetLength <= 1) receivedData.Reset(true);
        }
        catch (Exception e)
        {
            Disconnect();
            // TODO: handle game server disconnect
            WriteLine($"Error handling game server data: {e}");
        }
    }

    private async Task StartPinging()
    {
        try
        {
            // dispose is required when reconnects happen to disallow duplicates
            pingTimer?.Dispose();

            pingTimer = new Timer(async _ =>
            {
                using var packet = new Packet();
                packet.Write((int)Packets.ping);
                packet.Write(true);

                if (tcpClient is not null)
                {
                    packet.WriteLength();
                    await stream.WriteAsync(packet.ToArray());
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            await Task.Delay(Timeout.Infinite);
        }
        catch (Exception e)
        {
            Disconnect();
            WriteLine($"Game server disconnected: {e}");
        }
    }

    private void Disconnect()
    {
        tcpClient.Close();
        stream = null;
        receiveBuffer = null;
        receivedData = null;
        tcpClient = null;

        Server.games.Clear();
    }
}