using System.Net.Sockets;
using System.Net;

namespace game_server.Tcp;

public class Client(PacketRouter router, ILogger<Client> logger)
{
    private static TcpClient tcpClient;

    private static Packet receivedData;
    private static NetworkStream stream;
    private static byte[] receiveBuffer;

    private readonly int bufferSize = 4096;

    public async Task HandleConnect(IPAddress ip, int port)
    {
        while (tcpClient == null || !tcpClient.Connected)
        {
            try
            {
                tcpClient = new();
                await tcpClient.ConnectAsync(ip, port);
                logger.LogInformation("Connected to game server");

                stream = tcpClient.GetStream();
                receiveBuffer = new byte[bufferSize];
                receivedData = new();

                await RecieveData();
            }
            catch (Exception e)
            {
                logger.LogInformation("Error connecting to game server {}", e);
                await Task.Delay(TimeSpan.FromSeconds(5));
                logger.LogInformation("Reconnecting to game server");
            }
        }
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
                    logger.LogInformation("Matchmaking server disconnected");
                }

                var data = new byte[length];
                Array.Copy(receiveBuffer, data, length);

                _ = HandleData(data);
            }
        }
        catch (Exception e)
        {
            Disconnect();
            logger.LogInformation("Error recieving Matchmaking server data: {}", e);
        }
    }

    public static async Task SendData(Packet packet)
    {
        try
        {
            if (tcpClient is not null)
            {
                packet.WriteLength();
                await stream.WriteAsync(packet.ToArray());
            }
        }
        catch (Exception)
        {
            Disconnect();
        }
    }

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

                await router.Route(bytes);

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
            logger.LogInformation("Error handling matchmaking server data: {}", e);
            Disconnect();
        }
    }

    private static void Disconnect()
    {
        tcpClient.Close();
        stream = null;
        receiveBuffer = null;
        receivedData = null;
    }
}
