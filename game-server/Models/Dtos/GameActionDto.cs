using game_server.Models.Constants;

namespace game_server.Models.Dtos;

public record GameActionDto(GameAction Action, int From, int To);
