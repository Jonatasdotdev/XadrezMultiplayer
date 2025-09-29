using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Shared;

namespace Server.Services;

public class ClientHandler
{
    private readonly TcpClient _client;
    private readonly GameServer _server;
    private readonly ILogger _logger;
    private NetworkStream _stream;
    private StreamReader _reader;
    private StreamWriter _writer;
    
    public string ClientId { get; private set; }
    public string? Username { get; set; }
    public bool IsAuthenticated { get; set; }
    public GameSession? CurrentGame { get; set; }

    public ClientHandler(TcpClient client, GameServer server, ILogger logger)
    {
        _client = client;
        _server = server;
        _logger = logger;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
        ClientId = Guid.NewGuid().ToString();
    }

    public async Task HandleClientAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Cliente conectado: {ClientId}", ClientId);
            
            await SendMessageAsync(new { type = "welcome", clientId = ClientId, message = "Conectado ao servidor de Xadrez" });

            string? message;
            while (!cancellationToken.IsCancellationRequested && 
                   (message = await ReadMessageWithTimeoutAsync(cancellationToken)) != null)
            {
                _logger.LogDebug("Mensagem de {ClientId}: {Message}", ClientId, message);
                await ProcessMessageAsync(message);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Cliente {ClientId} desconectado (cancellation)", ClientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro com cliente {ClientId}", ClientId);
        }
        finally
        {
            await HandleDisconnectionAsync();
        }
    }

    private async Task<string?> ReadMessageWithTimeoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            var readTask = _reader.ReadLineAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            
            var completedTask = await Task.WhenAny(readTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                await SendMessageAsync(new { type = "timeout_warning" });
                return null;
            }
            
            return await readTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Timeout/erro na leitura do cliente {ClientId}", ClientId);
            return null;
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(message);
            var root = jsonDoc.RootElement;
            
            if (!root.TryGetProperty("type", out var typeProperty))
            {
                await SendMessageAsync(new { type = "error", message = "Campo 'type' não encontrado" });
                return;
            }

            var messageType = typeProperty.GetString();
            
            switch (messageType)
            {
                case "login":
                    await HandleLoginAsync(root);
                    break;
                case "ping":
                    await SendMessageAsync(new { type = "pong", timestamp = DateTime.UtcNow });
                    break;
                case "get_online_users":
                    await HandleGetOnlineUsersAsync();
                    break;
                default:
                    await SendMessageAsync(new { 
                        type = "error", 
                        message = $"Tipo de mensagem desconhecido: {messageType}" 
                    });
                    break;
            }
        }
        catch (JsonException ex)
        {
            await SendMessageAsync(new { 
                type = "error", 
                message = "JSON inválido: " + ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem do cliente {ClientId}", ClientId);
            await SendMessageAsync(new { 
                type = "error", 
                message = "Erro interno do servidor" 
            });
        }
    }

    private async Task HandleLoginAsync(JsonElement data)
    {
        try
        {
            var username = data.GetProperty("username").GetString();
            var password = data.GetProperty("password").GetString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await SendMessageAsync(new { 
                    type = "login_failed", 
                    message = "Username e password são obrigatórios" 
                });
                return;
            }

            // TODO: Implementar validação segura com database
            // Por enquanto, autenticação simples
            Username = username;
            IsAuthenticated = true;
            
            _logger.LogInformation("Usuário autenticado: {Username}", username);
            
            await SendMessageAsync(new { 
                type = "login_success", 
                username = username,
                onlineUsers = _server.GetOnlineUsers()
            });
            
            // Notificar outros clientes
            _server.BroadcastMessage(JsonSerializer.Serialize(new {
                type = "user_online",
                username = username
            }), this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login do cliente {ClientId}", ClientId);
            await SendMessageAsync(new { 
                type = "login_failed", 
                message = "Erro durante autenticação" 
            });
        }
    }

    private async Task HandleGetOnlineUsersAsync()
    {
        var onlineUsers = _server.GetOnlineUsers();
        await SendMessageAsync(new {
            type = "online_users",
            users = onlineUsers
        });
    }

    public async Task SendMessageAsync(object message)
    {
        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            await _writer.WriteLineAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar mensagem para {ClientId}", ClientId);
            await HandleDisconnectionAsync();
        }
    }

    private async Task HandleDisconnectionAsync()
    {
        try
        {
            // Notificar partida atual sobre desconexão
            if (CurrentGame != null)
            {
                // TODO: Implementar lógica de desconexão da partida
            }

            // Remover cliente do servidor
            _server.RemoveClient(this);

            // Notificar outros clientes
            if (!string.IsNullOrEmpty(Username))
            {
                _server.BroadcastMessage(JsonSerializer.Serialize(new {
                    type = "user_offline",
                    username = Username
                }), this);
            }

            _logger.LogInformation("🔌 Cliente desconectado: {ClientId} ({Username})", ClientId, Username);

            // Fechar conexões
            _reader?.Dispose();
            _writer?.Dispose();
            _stream?.Dispose();
            _client?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante desconexão de {ClientId}", ClientId);
        }
    }
}