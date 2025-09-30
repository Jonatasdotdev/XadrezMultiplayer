using Server.Models;
using Server.Services;

namespace Server.Services
{
    public class GameSessionManager
    {
        private readonly Dictionary<string, GameSession> _gameSessions = new();
        private readonly Dictionary<string, GameInvite> _invites = new();
        private readonly object _lock = new();

        public GameSession CreateGameSession(ClientHandler whitePlayer, ClientHandler blackPlayer)
        {
            lock (_lock)
            {
                var gameSession = new GameSession(whitePlayer, blackPlayer)
                {
                    Player2 = blackPlayer,
                    Player2Username = blackPlayer.State?.Username ?? string.Empty
                };
                _gameSessions[gameSession.SessionId] = gameSession; 
                return gameSession;
            }
        }

        public void AddInvite(GameInvite invite)
        {
            lock (_lock)
            {
                _invites[invite.InviteId] = invite;
            }
        }

        public GameInvite? GetInvite(string inviteId)
        {
            lock (_lock)
            {
                return _invites.GetValueOrDefault(inviteId);
            }
        }

        public void RemoveInvite(string inviteId)
        {
            lock (_lock)
            {
                _invites.Remove(inviteId);
            }
        }

        public void RemoveGameSession(string sessionId)
        {
            lock (_lock)
            {
                _gameSessions.Remove(sessionId);
            }
        }
    }
}