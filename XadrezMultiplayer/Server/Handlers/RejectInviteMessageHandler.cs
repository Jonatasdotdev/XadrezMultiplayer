using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class RejectInviteMessageHandler : IMessageHandler
{
    public string MessageType => "reject_invite";
    private readonly GameSessionManager _gameSessionManager;
    private readonly ILogger _logger;

    public RejectInviteMessageHandler(GameSessionManager gameSessionManager, ILogger logger)
    {
        _gameSessionManager = gameSessionManager;
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        try
        {
            var inviteId = data.GetProperty("inviteId").GetString();
            var invite = _gameSessionManager.GetInvite(inviteId!);

            if (invite != null && invite.ToPlayer == clientHandler.State.Username)
            {
                var fromClient = clientHandler.Server.GetClientByUsername(invite.FromPlayer);
                if (fromClient != null)
                {
                    await fromClient.SendMessageAsync(new {
                        type = "invite_rejected",
                        byPlayer = clientHandler.State.Username
                    });
                }

                _gameSessionManager.RemoveInvite(inviteId!);
            }

            await clientHandler.SendMessageAsync(new {
                type = "invite_rejected_success"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar convite do cliente {ClientId}", clientHandler.State.ClientId);
        }
    }
}