using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Services;

public class MessageProcessor
{
    private readonly Dictionary<string, IMessageHandler> _handlers;
    private readonly ILogger _logger;

    public MessageProcessor(IEnumerable<IMessageHandler> handlers, ILogger logger)
    {
        _logger = logger;
        _handlers = new Dictionary<string, IMessageHandler>();
        foreach (var handler in handlers)
        {
            _handlers[handler.MessageType] = handler;
        }
    }

    public async Task ProcessAsync(string message, ClientHandler clientHandler)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(message);
            var root = jsonDoc.RootElement;
            
            if (!root.TryGetProperty("type", out var typeProperty))
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "error", 
                    message = "Campo 'type' não encontrado" 
                });
                return;
            }

            var messageType = typeProperty.GetString();
            
            if (_handlers.TryGetValue(messageType!, out var handler))
            {
                await handler.HandleAsync(root, clientHandler);
            }
            else
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "error", 
                    message = $"Tipo de mensagem desconhecido: {messageType}" 
                });
            }
        }
        catch (JsonException ex)
        {
            await clientHandler.SendMessageAsync(new { 
                type = "error", 
                message = "JSON inválido: " + ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem do cliente {ClientId}", clientHandler.State.ClientId);
            await clientHandler.SendMessageAsync(new { 
                type = "error", 
                message = "Erro interno do servidor" 
            });
        }
    }
}