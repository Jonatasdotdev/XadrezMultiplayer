using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class MakeMoveMessageHandler : IMessageHandler
{
    public string MessageType => "make_move";
    private readonly ILogger _logger;

    public MakeMoveMessageHandler(ILogger logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        if (!clientHandler.State.IsAuthenticated || clientHandler.State.CurrentGame == null)
        {
            await clientHandler.SendMessageAsync(new { 
                type = "error", 
                message = "Não está em uma partida" 
            });
            return;
        }

        try
        {
            var from = data.GetProperty("from").GetString();
            var to = data.GetProperty("to").GetString();
            var promotion = data.TryGetProperty("promotion", out var prop) ? prop.GetString() : null;

            var moveResult = clientHandler.State.CurrentGame.MakeMove(clientHandler, from!, to!, promotion);

            if (moveResult.IsValid)
            {
                var moveMessage = new {
                    type = "move_made",
                    from = from,
                    to = to,
                    player = clientHandler.State.Username,
                    promotion = promotion,
                    gameState = moveResult.GameState,
                    board = clientHandler.State.CurrentGame.GetBoardState(),
                    currentTurn = clientHandler.State.CurrentGame.CurrentTurnPlayer?.State.Username,
                    isCheck = moveResult.IsCheck,
                    isCheckmate = moveResult.IsCheckmate,
                    isDraw = moveResult.IsDraw
                };

                await clientHandler.State.CurrentGame.BroadcastToPlayers(moveMessage);

                if (moveResult.IsCheckmate || moveResult.IsDraw)
                {
                    await clientHandler.HandleGameEndAsync(moveResult);
                }
            }
            else
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "invalid_move", 
                    message = moveResult.ErrorMessage 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar movimento do cliente {ClientId}", clientHandler.State.ClientId);
            await clientHandler.SendMessageAsync(new { 
                type = "error", 
                message = "Erro ao processar movimento" 
            });
        }
    }
}