using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Log
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [MaxLength(50)]
        public string? Username { get; set; } // Associado a um usuário
    }
}