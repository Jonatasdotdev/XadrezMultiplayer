using Microsoft.Extensions.Logging;
using System.Text.Json;
using Server.Services;

namespace Server.Handlers
{
    public class RegisterMessageHandler : IMessageHandler
    {
        public string MessageType => "register";

        private readonly AuthService _authService;
        private readonly ILogger<RegisterMessageHandler> _logger;

        public RegisterMessageHandler(AuthService authService, ILogger<RegisterMessageHandler> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
        {
            try
            {
                var username = data.GetProperty("username").GetString();
                var password = data.GetProperty("password").GetString();
                var email = data.GetProperty("email").GetString() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    await clientHandler.SendMessageAsync(new { type = "register_failed", message = "Username e password são obrigatórios" });
                    return;
                }

                var result = await _authService.RegisterUserAsync(username, password, email);
                if (result.IsSuccess)
                {
                    clientHandler.State.Username = username;
                    clientHandler.State.IsAuthenticated = true;
                    await clientHandler.SendMessageAsync(new { type = "register_success", message = "Usuário registrado com sucesso" });
                }
                else
                {
                    await clientHandler.SendMessageAsync(new { type = "register_failed", message = result.ErrorMessage });
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Erro ao parsear mensagem de registro para {ClientId}", clientHandler.State.ClientId);
                await clientHandler.SendMessageAsync(new { type = "register_failed", message = "JSON inválido" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar registro para {ClientId}", clientHandler.State.ClientId);
                await clientHandler.SendMessageAsync(new { type = "register_failed", message = "Erro interno" });
            }
        }
    }
}