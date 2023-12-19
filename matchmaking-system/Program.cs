using System.Net;
using matchmaking_system.Servers;

Console.Title = "Matchmaking Server";

var server = new Server(IPAddress.Loopback, 13);
await server.Start();