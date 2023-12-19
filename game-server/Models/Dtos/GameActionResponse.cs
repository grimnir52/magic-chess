using game_server.Models.Constants;

namespace game_server.Models.Dtos;

public record GameActionResponse(Color? Color, PieceType? Type, Color Turn, bool? IsCheckmate);
