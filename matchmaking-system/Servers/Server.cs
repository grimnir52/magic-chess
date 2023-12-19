using static System.Console;
using System.Net;
using System.Net.Sockets;
using matchmaking_system.Clients;
using matchmaking_system.Models;

namespace matchmaking_system.Servers;

public class Server(IPAddress ip, int port)
{
    private TcpListener tcpListener;
    private UdpClient udpListener;

    public static readonly GameServer gameServer = new();
    public static readonly FrontEnd frontEnd = new();
    public static readonly List<Game> games = [];

    public async Task Start()
    {
        WriteLine("Server starting...");

        tcpListener = new(ip, port);
        tcpListener.Start();

        WriteLine($"Server started on host: {ip}:{port}");

        udpListener = new(port);

        List<Task> tasks = [AcceptTcp(), AcceptUdp()];
        await Task.WhenAll(tasks);
    }

    private async Task AcceptTcp()
    {
        while (!gameServer.IsConnected())
        {
            WriteLine("Waiting for game server to connect using tcp");

            var tcpClient = await tcpListener.AcceptTcpClientAsync();

            WriteLine($"Game server connected from host: {tcpClient.Client.RemoteEndPoint}");

            await gameServer.HandleConnect(tcpClient);
        }
    }

    private async Task AcceptUdp()
    {
        while (!frontEnd.IsConnected())
        {
            WriteLine("Waiting for front end server to connect using udp");

            var result = await udpListener.ReceiveAsync();

            WriteLine($"Front end Server connected from host: {result.RemoteEndPoint}");

            await frontEnd.HandleConnect(udpListener, result);
        }
    }
}
