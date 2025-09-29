using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class RespondDrawMessageHandler : IMessageHandler
{
    public string MessageType => "respond_draw";
    private readonly ILogger _logger;

    public RespondDrawMessageHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        if (clientHandler.State.CurrentGame == null)
        {
            await clientHandler.SendMessageAsync(new { 
                type = "error", 
                message = "Não está em uma partida" 
            });
            return;
        }

        try
        {
            var accept = data.GetProperty("accept").GetBoolean();
            var opponent = clientHandler.State.CurrentGame.GetOpponent(clientHandler);

            if (accept && opponent != null)
            {
                await clientHandler.State.CurrentGame.HandleDraw(clientHandler, opponent);
            }
            else if (opponent != null)
            {
                await opponent.SendMessageAsync(new {
                    type = "draw_rejected",
                    byPlayer = clientHandler.State.Username
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao responder empate do cliente {ClientId}", clientHandler.State.ClientId);
        }
    }
}