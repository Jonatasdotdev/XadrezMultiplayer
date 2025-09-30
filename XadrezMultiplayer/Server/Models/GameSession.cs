using Server.Services;
using System;

namespace Server.Models
{
    public class GameSession
    {
        public string SessionId { get; } = Guid.NewGuid().ToString();
        public string Player1Username { get; set; }
        public required string Player2Username { get; set; }
        public ClientHandler Player1 { get; set; }
        public required ClientHandler Player2 { get; set; }
        public bool IsPlayer1White { get; set; }
        public DateTime Created { get; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public object CurrentTurnPlayer { get; private set; }

        public GameSession(ClientHandler player1, ClientHandler player2)
        {
            Player1 = player1;
            Player2 = player2;
            Player1Username = player1.State.Username ?? throw new ArgumentNullException(nameof(player1.State.Username));
            Player2Username = player2.State.Username ?? throw new ArgumentNullException(nameof(player2.State.Username));
            IsPlayer1White = Random.Shared.Next(2) == 0; // Randomly assign colors

            CurrentTurnPlayer = IsPlayer1White ? Player1 : Player2;
        }

        public void SwitchTurn()
    {
        CurrentTurnPlayer = CurrentTurnPlayer == Player1 ? Player2 : Player1;
    }

    public bool IsPlayerTurn(ClientHandler player)
    {
        return player == CurrentTurnPlayer;
    }

        public bool HasPlayer(string username)
        {
            return Player1Username == username || Player2Username == username;
        }

        public bool IsFull()
        {
            return Player1Username != null && Player2Username != null;
        }

        public ClientHandler GetOpponent(ClientHandler player)
        {
            return player == Player1 ? Player2 : Player1;
        }

        public string GetBoardState()
        {
            //TODO substituir por lógica real de tabuleiro (ex.: FEN ou matriz)
            return "initial_board_state"; // Placeholder
        }

        public async Task BroadcastToPlayers(object message)
        {
            await Player1.SendMessageAsync(message);
            await Player2.SendMessageAsync(message);
        }

        public void HandlePlayerDisconnect(ClientHandler player)
        {
            IsActive = false;
            // TODO Adicionar lógica para notificar o oponente ou encerrar a sessão
        }

          public async Task HandleResignation(ClientHandler clientHandler)
        {
            //  TODO Implement resignation logic here, for example:
            // Mark the game as resigned, notify other players, etc.
            await Task.CompletedTask;
        }

        public async Task HandleDraw(ClientHandler requester, ClientHandler opponent)
        {
            // Notify both players about the draw
            await requester.SendMessageAsync(new
            {
                type = "draw_accepted",
                byPlayer = requester.State.Username
            });

            await opponent.SendMessageAsync(new
            {
                type = "draw_accepted",
                byPlayer = requester.State.Username
            });

            // update game state to reflect the draw
            // this.Status = GameStatus.Draw;
        }

        public MoveResult MakeMove(ClientHandler player, string from, string to, string? promotion)
        {
            if (!IsPlayerTurn(player))
        {
            return new MoveResult { IsValid = false, ErrorMessage = "Não é sua vez" };
        }
            //  TODO Implement move logic here Validate it.
            // For now, return a dummy MoveResult to avoid compile errors.
            return new MoveResult
            {
                IsValid = false,
                ErrorMessage = "Not implemented",
                GameState = string.Empty,
                IsCheck = false,
                IsCheckmate = false,
                IsDraw = false
            };
        }
        
    }
}
    

    
