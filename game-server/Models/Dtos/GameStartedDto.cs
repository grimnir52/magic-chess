using game_server.Models.Constants;

namespace game_server.Models.Dtos;

public record GameStartedDto(Dictionary<string, Player> Players, Color Turn, State State);
