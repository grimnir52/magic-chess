using game_server.Models;
using game_server.Models.Constants;
using game_server.Models.Dtos;
using game_server.Repos;
using matchmaking_system.Handlers;

namespace game_server.Services;

public class GameService(GameTableRepo tableRepo)
{
    public async Task<GameTable> Create(string id)
    {
        var table = new GameTable(id);
        await tableRepo.Add(table);
        return table;
    }

    public async Task<Dictionary<string, Player>> Join(string id, string playerId)
    {
        var table = await tableRepo.Get(id);
        if (table.State != State.pending) return null;

        if (table == null) return null;

        var players = table.Players;

        Player player;
        if (players.Count != 0)
            player = new(Color.black, table.Board.PiecesPerPlayer);
        else
            player = new(Color.red, table.Board.PiecesPerPlayer);

        players[playerId] = player;
        await tableRepo.Update(table);
        return players;
    }

    public async Task<bool?> Ready(string id, string playerId)
    {
        var table = await tableRepo.Get(id);
        if (table.State != State.pending) return null;

        bool hasStarted = false;

        if (table == null) return null;

        if (table.Players.TryGetValue(playerId, out Player player))
        {
            player.Ready = !player.Ready;

            if (table.Players.Count(p => p.Value.Ready) == table.MaxPlayers)
            {
                table.State = State.started;
                hasStarted = true;
            }

            await tableRepo.Update(table);
        }

        return hasStarted;
    }

    public async Task<GameActionResponse> PlayerAction(string id, string playerId, GameActionDto dto)
    {
        var table = await tableRepo.Get(id);
        if (table.State != State.started) return null;

        if (table.Players.TryGetValue(playerId, out Player player))
        {
            if (player.Color != table.Turn) return null;
            if (dto.From == 0) return null;

            bool? isCheckmate;
            if (dto.Action == GameAction.flip)
                isCheckmate = table.Board.FlipPiece(dto.From, player.Color);
            else isCheckmate = table.Board.MovePiece(dto.From, dto.To, player.Color);


            table.Turn = player.Color == Color.black ? Color.red : Color.black;

            if (isCheckmate == null) return null;
            else if (isCheckmate == true)
            {
                table.State = State.finished;
                _ = PacketSender.SendGameFinished(id);
            }

            await tableRepo.Update(table);

            Color? color = dto.Action == GameAction.flip ? player.Color : null;
            var type = dto.Action == GameAction.flip ? table.Board.Pieces[dto.From].Type : null;
            return new GameActionResponse(color, type, table.Turn, isCheckmate);
        }

        return null;
    }
}
