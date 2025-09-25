namespace Server.Models;

public class Board
{
    public Piece[,] Grid { get; set; } = new Piece[8, 8];

    public Board()
    {
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        // Inicializa peças brancas
        Grid[0, 0] = new Piece { Type = PieceType.Rook, Color = Color.White };
        Grid[0, 1] = new Piece { Type = PieceType.Knight, Color = Color.White };
        Grid[0, 2] = new Piece { Type = PieceType.Bishop, Color = Color.White };
        Grid[0, 3] = new Piece { Type = PieceType.Queen, Color = Color.White };
        Grid[0, 4] = new Piece { Type = PieceType.King, Color = Color.White };
        Grid[0, 5] = new Piece { Type = PieceType.Bishop, Color = Color.White };
        Grid[0, 6] = new Piece { Type = PieceType.Knight, Color = Color.White };
        Grid[0, 7] = new Piece { Type = PieceType.Rook, Color = Color.White };

        for (int i = 0; i < 8; i++)
            Grid[1, i] = new Piece { Type = PieceType.Pawn, Color = Color.White };

        // Inicializa peças pretas
        Grid[7, 0] = new Piece { Type = PieceType.Rook, Color = Color.Black };
        Grid[7, 1] = new Piece { Type = PieceType.Knight, Color = Color.Black };
        Grid[7, 2] = new Piece { Type = PieceType.Bishop, Color = Color.Black };
        Grid[7, 3] = new Piece { Type = PieceType.Queen, Color = Color.Black };
        Grid[7, 4] = new Piece { Type = PieceType.King, Color = Color.Black };
        Grid[7, 5] = new Piece { Type = PieceType.Bishop, Color = Color.Black };
        Grid[7, 6] = new Piece { Type = PieceType.Knight, Color = Color.Black };
        Grid[7, 7] = new Piece { Type = PieceType.Rook, Color = Color.Black };

        for (int i = 0; i < 8; i++)
            Grid[6, i] = new Piece { Type = PieceType.Pawn, Color = Color.Black };
    }
}