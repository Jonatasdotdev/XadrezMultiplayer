using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Services;

public class AcceptInviteMessageHandler : IMessageHandler
{
    public string MessageType => "accept_invite";
    private readonly GameSessionManager _gameSessionManager;
    private readonly ILogger _logger;

    public AcceptInviteMessageHandler(GameSessionManager gameSessionManager, ILogger logger)
    {
        _gameSessionManager = gameSessionManager;
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        if (!clientHandler.State.IsAuthenticated)
        {
            await clientHandler.SendMessageAsync(new { 
                type = "error", 
                message = "Autenticação necessária" 
            });
            return;
        }

        try
        {
            var inviteId = data.GetProperty("inviteId").GetString();
            var invite = _gameSessionManager.GetInvite(inviteId!);

            if (invite == null || invite.ToPlayer != clientHandler.State.Username)
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "invite_accept_failed", 
                    message = "Convite inválido ou expirado" 
                });
                return;
            }

            var fromClient = clientHandler.Server.GetClientByUsername(invite.FromPlayer);
            if (fromClient == null || fromClient.State.CurrentGame != null)
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "invite_accept_failed", 
                    message = "Jogador que convidou não está mais disponível" 
                });
                return;
            }

            var gameSession = _gameSessionManager.CreateGameSession(fromClient, clientHandler);
            fromClient.State.CurrentGame = gameSession;
            clientHandler.State.CurrentGame = gameSession;

            _gameSessionManager.RemoveInvite(inviteId!);

            var gameStartMessage = new {
                type = "game_started",
                gameId = gameSession.GameId,
                whitePlayer = fromClient.State.Username,
                blackPlayer = clientHandler.State.Username,
                board = gameSession.GetBoardState(),
                currentTurn = fromClient.State.Username // Brancas começam
            };

            await fromClient.SendMessageAsync(gameStartMessage);
            await clientHandler.SendMessageAsync(gameStartMessage);

            _logger.LogInformation("♟️ Partida iniciada: {White} vs {Black}", fromClient.State.Username, clientHandler.State.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar convite do cliente {ClientId}", clientHandler.State.ClientId);
            await clientHandler.SendMessageAsync(new { 
                type = "invite_accept_failed", 
                message = "Erro ao aceitar convite" 
            });
        }
    }
}