namespace Client.Models;

public class Player
{
    public string Username { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public int GamesWon { get; set; }
    public int GamesLost { get; set; }
}