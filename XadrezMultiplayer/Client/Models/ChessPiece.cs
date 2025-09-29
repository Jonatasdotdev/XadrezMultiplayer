using System.Windows.Media;

namespace Client.Models;

public enum PieceType { King, Queen, Rook, Bishop, Knight, Pawn }
public enum PieceColor { White, Black }

public class ChessPiece
{
    public PieceType Type { get; set; }
    public PieceColor Color { get; set; }
    public string Symbol => GetSymbol();
    public bool HasMoved { get; set; }

    private string GetSymbol()
    {
        return Type switch
        {
            PieceType.King => Color == PieceColor.White ? "♔" : "♚",
            PieceType.Queen => Color == PieceColor.White ? "♕" : "♛",
            PieceType.Rook => Color == PieceColor.White ? "♖" : "♜",
            PieceType.Bishop => Color == PieceColor.White ? "♗" : "♝",
            PieceType.Knight => Color == PieceColor.White ? "♘" : "♞",
            PieceType.Pawn => Color == PieceColor.White ? "♙" : "♟",
            _ => "?"
        };
    }
}