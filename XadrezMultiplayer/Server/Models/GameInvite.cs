using System.ComponentModel.DataAnnotations;

namespace Server.Models;

public class GameInvite
{
    [Key]
    public string InviteId { get; set; } = string.Empty;
    public string FromPlayer { get; set; } = string.Empty;
    public string ToPlayer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt => CreatedAt.AddMinutes(5); // Convite expira em 5 minutos
}