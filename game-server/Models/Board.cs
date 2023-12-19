using game_server.Models.Constants;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace game_server.Models;

public class Board
{
    public int RowsCount { get; set; } = 4;
    public int ColumnsCount { get; set; } = 8;
    public int PiecesPerPlayer { get; set; } = 16;

    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
    public Dictionary<PieceType, int> PiecesPerType { get; set; } = [];

    [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
    public Dictionary<int, Piece> Pieces { get; set; } = [];

    public List<Piece> PiecesCaptured { get; set; } = [];


    [BsonIgnore]
    public IEnumerable<Piece> AllPieces => [.. Pieces.Values, .. PiecesCaptured];

    [BsonIgnore]
    public int TotalPieces => RowsCount * ColumnsCount;

    public Board()
    {
        PiecesPerType.Add(PieceType.king, 1);
        PiecesPerType.Add(PieceType.adviser, 2);
        PiecesPerType.Add(PieceType.bishop, 2);
        PiecesPerType.Add(PieceType.rook, 2);
        PiecesPerType.Add(PieceType.knight, 2);
        PiecesPerType.Add(PieceType.cannon, 2);
        PiecesPerType.Add(PieceType.pawn, 5);
    }

    public int[] GetDirections(int postition, int columns)
    {
        var up = postition - columns;
        var right = postition + 1;
        var down = postition + columns;
        var left = postition - 1;
        return [up, right, down, left];
    }

    public bool? FlipPiece(int position, Color color)
    {
        if (Pieces.GetValueOrDefault(position) != null) return null;

        var playerTypes = AllPieces.Where(p => p.Color == color)
                .Select(p => p.Type).ToList();

        var random = new Random();
        var types = Enum.GetValues<PieceType>();
        PieceType pieceType;
        do
        {
            var i = random.Next(0, types.Length);
            pieceType = types[i];
        } while (playerTypes.Count(t => t == pieceType) >= PiecesPerType[pieceType]);

        Pieces[position] = new(pieceType, true, color);
        return false;
    }

    public bool? MovePiece(int from, int to, Color color)
    {
        if (from is 0 || to is 0) return null;

        if (!Pieces.TryGetValue(from, out Piece fromPiece) || !fromPiece.IsFlipped)
            return null;

        if (!GetDirections(from, ColumnsCount).Contains(to))
            return null;

        bool isCheckmate = false;
        if (Pieces.TryGetValue(to, out Piece toPiece))
        {
            if (toPiece.Color == color) return null;

            isCheckmate = (fromPiece.Type == PieceType.pawn && toPiece.Type == PieceType.king)
            || (fromPiece.Type == PieceType.king && toPiece.Type == PieceType.king);

            if (fromPiece.Type > toPiece.Type && !isCheckmate) return null;

            PiecesCaptured.Add(toPiece);
            Pieces[to] = fromPiece;
            Pieces.Remove(from);
        }
        else
        {
            Pieces.Add(to, fromPiece);
            Pieces.Remove(from);
        }

        return isCheckmate;
    }
}
