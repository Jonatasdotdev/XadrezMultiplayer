using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Handlers;

public class RegisterMessageHandler : IMessageHandler
{
    public string MessageType => "register";
    private readonly AuthService _authService;
    private readonly ILogger _logger;

    public RegisterMessageHandler(AuthService authService, ILogger logger)
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
            var email = data.GetProperty("email").GetString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "register_failed", 
                    message = "Username e password são obrigatórios" 
                });
                return;
            }

            var registerResult = await _authService.RegisterUserAsync(username, password, email);
            
            if (registerResult.IsSuccess)
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "register_success", 
                    message = "Usuário registrado com sucesso" 
                });
            }
            else
            {
                await clientHandler.SendMessageAsync(new { 
                    type = "register_failed", 
                    message = registerResult.ErrorMessage 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no registro do cliente {ClientId}", clientHandler.State.ClientId);
            await clientHandler.SendMessageAsync(new { 
                type = "register_failed", 
                message = "Erro durante registro" 
            });
        }
    }
}