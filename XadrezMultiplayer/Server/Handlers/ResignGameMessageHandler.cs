using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class ResignGameMessageHandler : IMessageHandler
{
    public string MessageType => "resign_game";
    private readonly ILogger<ResignGameMessageHandler> _logger;

    public ResignGameMessageHandler(ILogger<ResignGameMessageHandler> logger)
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
            await clientHandler.State.CurrentGame.HandleResignation(clientHandler);

            _logger.LogInformation("🏳️ Jogador {Player} desistiu da partida", clientHandler.State.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar desistência do cliente {ClientId}", clientHandler.State.ClientId);
        }
    }
}