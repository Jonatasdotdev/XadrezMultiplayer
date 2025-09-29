using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class OfferDrawMessageHandler : IMessageHandler
{
    public string MessageType => "offer_draw";
    private readonly ILogger _logger;

    public OfferDrawMessageHandler(ILogger logger)
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
            var opponent = clientHandler.State.CurrentGame.GetOpponent(clientHandler);
            if (opponent != null)
            {
                await opponent.SendMessageAsync(new {
                    type = "draw_offered",
                    fromPlayer = clientHandler.State.Username
                });
            }

            await clientHandler.SendMessageAsync(new {
                type = "draw_offer_sent"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao oferecer empate do cliente {ClientId}", clientHandler.State.ClientId);
        }
    }
}