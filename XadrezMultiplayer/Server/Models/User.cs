using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class User
{
    [Key]
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
}