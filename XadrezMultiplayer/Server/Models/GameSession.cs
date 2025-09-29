using System.Net.Sockets;

namespace Server.Models
{
    public class GameSession
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public string Player1Username { get; set; }
        public string Player2Username { get; set; }
        public TcpClient Player1Client { get; set; }
        public TcpClient Player2Client { get; set; }
        public bool IsPlayer1White { get; set; }
        public DateTime Created { get; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public GameSession(string player1Username, TcpClient player1Client)
        {
            Player1Username = player1Username;
            Player1Client = player1Client;
            IsPlayer1White = Random.Shared.Next(2) == 0; // Randomly assign colors
        }

        public bool HasPlayer(string username)
        {
            return Player1Username == username || Player2Username == username;
        }

        public bool IsFull()
        {
            return Player1Username != null && Player2Username != null;
        }
    }
}