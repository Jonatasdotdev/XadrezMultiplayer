using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared;
using Server.Services;
using Server.Models;
using System.Text.Json;

namespace Server.Handlers;

public class InvitePlayerMessageHandler : IMessageHandler
{
    public string MessageType => "invite_player";
    private readonly GameSessionManager _gameSessionManager;
    private readonly ILogger<InvitePlayerMessageHandler> _logger;

    public InvitePlayerMessageHandler(GameSessionManager gameSessionManager, ILogger<InvitePlayerMessageHandler> logger)
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
            var targetUsername = data.GetProperty("targetUsername").GetString();
            var targetClient = clientHandler.Server.GetClientByUsername(targetUsername!);

            if (targetClient == null || !targetClient.State.IsAuthenticated)
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "invite_failed", 
                    message = "Jogador não encontrado ou offline" 
                });
                return;
            }

            if (targetClient.State.CurrentGame != null)
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "invite_failed", 
                    message = "Jogador já está em uma partida" 
                });
                return;
            }

            var inviteId = Guid.NewGuid().ToString();
            var invite = new GameInvite
            {
                InviteId = inviteId,
                FromPlayer = clientHandler.State.Username!,
                ToPlayer = targetUsername!,
                CreatedAt = DateTime.UtcNow
            };

            _gameSessionManager.AddInvite(invite);

            await targetClient.SendMessageAsync(new {
                type = "invite_received",
                inviteId = inviteId,
                fromPlayer = clientHandler.State.Username,
                timestamp = DateTime.UtcNow
            });

            await clientHandler.SendMessageAsync(new {
                type = "invite_sent",
                targetPlayer = targetUsername
            });

            _logger.LogInformation("Convite enviado de {From} para {To}", clientHandler.State.Username, targetUsername);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar convite do cliente {ClientId}", clientHandler.State.ClientId);
            await clientHandler.SendMessageAsync(new { 
                type = "invite_failed", 
                message = "Erro ao enviar convite" 
            });
        }
    }
}