using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class LoginMessageHandler : IMessageHandler
{
    public string MessageType => "login";
    private readonly AuthService _authService;
    private readonly ILogger _logger;

    public LoginMessageHandler(AuthService authService, ILogger logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        try
        {
            var username = data.GetProperty("username").GetString();
            var password = data.GetProperty("password").GetString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "login_failed", 
                    message = "Username e password são obrigatórios" 
                });
                return;
            }

            var authResult = await _authService.AuthenticateUserAsync(username, password);
            
            if (!authResult.IsSuccess)
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "login_failed", 
                    message = authResult.ErrorMessage 
                });
                return;
            }

            clientHandler.State.Username = username;
            clientHandler.State.IsAuthenticated = true;
            
            _logger.LogInformation("Usuário autenticado: {Username}", username);
            
            await clientHandler.SendMessageAsync(new { 
                type = "login_success", 
                username = username,
                onlineUsers = clientHandler.Server.GetOnlineUsers()
            });
            
            clientHandler.Server.BroadcastMessage(JsonSerializer.Serialize(new {
                type = "user_online",
                username = username
            }), clientHandler);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login do cliente {ClientId}", clientHandler.State.ClientId);
            await clientHandler.SendMessageAsync(new { 
                type = "login_failed", 
                message = "Erro durante autenticação" 
            });
        }
    }
}