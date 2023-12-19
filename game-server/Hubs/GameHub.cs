using game_server.Models.Dtos;
using game_server.Services;
using Microsoft.AspNetCore.SignalR;

namespace game_server.Hubs;

// used for connecting to the js client web socket
public class GameHub(ILogger<GameHub> log, GameService gameService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var clientId = Context.ConnectionId;
        log.LogInformation("Client: {} Connected", clientId);

        var tableId = Context?.GetHttpContext()?.Request.Query
            .FirstOrDefault(q => q.Key == "token").Value.FirstOrDefault();

        if (tableId is null)
        {
            Context.Abort();
            return;
        }

        var players = await gameService.Join(tableId, clientId);
        if (players == null)
        {
            Context.Abort();
            return;
        }

        var notifyOthers = Clients.AllExcept(clientId).SendAsync("Join", new PlayerDto(clientId, players[clientId]));
        var response = Clients.Client(clientId).SendAsync("Connected", players);

        await Task.WhenAll(notifyOthers, response);
        await base.OnConnectedAsync();
    }

    //used to handle player start click
    public async Task Ready()
    {
        var clientId = Context.ConnectionId;
        var tableId = Context?.GetHttpContext()?.Request.Query
                    .FirstOrDefault(q => q.Key == "token").Value.FirstOrDefault();

        var hasStarted = await gameService.Ready(tableId, clientId);
        if (hasStarted == null)
        {
            Context.Abort();
            return;
        }
        else if (hasStarted == true)
        {
            await Clients.All.SendAsync("GameStarted");
        }
    }

    // used to handle player game actions
    public async Task<GameActionResponse> Action(GameActionDto dto)
    {
        var clientId = Context.ConnectionId;
        var tableId = Context?.GetHttpContext()?.Request.Query
                    .FirstOrDefault(q => q.Key == "token").Value.FirstOrDefault();

        var response = await gameService.PlayerAction(tableId, clientId, dto);

        if (response == null)
        {
            return null;
        }

        if (response.IsCheckmate == true)
        {
            await Clients.All.SendAsync("GameFinished");
            return null;
        }
        else
        {
            var broadcast = new GameActionBroadcast(dto.Action, response.Color, response.Type, response.Turn, dto.From, dto.To);
            await Clients.AllExcept(clientId).SendAsync("Action", broadcast);
        }

        return response;
    }
}