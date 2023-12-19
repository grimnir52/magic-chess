using game_server.Models.Constants;

namespace game_server.Models.Dtos;

public record GameActionBroadcast(GameAction Action, Color? Color, PieceType? Type, Color Turn, int From, int To);
