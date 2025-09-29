using System;
using Server.Models;

namespace Server.Services;

public class ClientState
{
    public string ClientId { get; } = Guid.NewGuid().ToString();
    public string? Username { get; set; }
    public bool IsAuthenticated { get; set; }
    public GameSession? CurrentGame { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}