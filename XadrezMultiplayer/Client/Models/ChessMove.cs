namespace Client.Models;

public class ChessMove
{
    public string From { get; set; } = string.Empty; // ex: "e2"
    public string To { get; set; } = string.Empty;   // ex: "e4"
    public ChessPiece? Piece { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}