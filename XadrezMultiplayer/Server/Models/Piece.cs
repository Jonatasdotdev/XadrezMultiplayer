namespace Server.Models;
public enum PieceType { King, Queen, Rook, Bishop, Knight, Pawn }
public enum Color { White, Black }

public class Piece
{
    public PieceType Type { get; set; }
    public Color Color { get; set; }
    public bool HasMoved { get; set; } = false;
}