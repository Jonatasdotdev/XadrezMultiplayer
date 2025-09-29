using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server.Handlers;

public class PingMessageHandler : IMessageHandler
{
    public string MessageType => "ping";

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        await clientHandler.SendMessageAsync(new { 
            type = "pong", 
            timestamp = DateTime.UtcNow,
            clientId = clientHandler.State.ClientId
        });
    }
}