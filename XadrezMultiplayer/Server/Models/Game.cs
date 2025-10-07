using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string WhitePlayerUsername { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string BlackPlayerUsername { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Winner { get; set; } // Pode ser nulo se for empate

        [Required]
        [MaxLength(20)]
        public string Result { get; set; } = "ongoing"; // ex.: "checkmate", "draw", "resign"

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        [MaxLength(100)]
        public string FenState { get; set; } = string.Empty; // Estado do tabuleiro em FEN
    }
}