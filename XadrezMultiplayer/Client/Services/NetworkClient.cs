using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;
using Client.Messages;

namespace Client.Services;

public class NetworkClient : IDisposable
{
    private TcpClient _tcpClient;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private StreamWriter? _writer;
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isConnected = false;

    public bool IsConnected => _isConnected && _tcpClient?.Connected == true;
    public string? ClientId { get; private set; }
    public string? Username { get; private set; }

    public NetworkClient()
    {
        _tcpClient = new TcpClient();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task<bool> ConnectAsync(string ip, int port)
    {
        try
        {
            await _tcpClient.ConnectAsync(ip, port);
            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
            
            _isConnected = true;
            
            // Iniciar escuta de mensagens
            _ = Task.Run(StartListeningAsync);
            
            return true;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage($"Falha na conexão: {ex.Message}"));
            return false;
        }
    }

    private async Task StartListeningAsync()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested && IsConnected)
        {
            try
            {
                var message = await _reader!.ReadLineAsync();
                if (message != null)
                {
                    await ProcessIncomingMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                if (!_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    WeakReferenceMessenger.Default.Send(new ErrorMessage($"Erro na conexão: {ex.Message}"));
                    Disconnect();
                }
                break;
            }
        }
    }

    private async Task ProcessIncomingMessageAsync(string message)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(message);
            var root = jsonDoc.RootElement;
            var messageType = root.GetProperty("type").GetString();

            switch (messageType)
            {
                case "welcome":
                    HandleWelcomeMessage(root);
                    break;
                case "login_success":
                    await HandleLoginSuccessAsync(root);
                    break;
                case "login_failed":
                    HandleLoginFailed(root);
                    break;
                case "online_users":
                    HandleOnlineUsers(root);
                    break;
                case "user_online":
                    HandleUserOnline(root);
                    break;
                case "user_offline":
                    HandleUserOffline(root);
                    break;
                case "pong":
                    HandlePong(root);
                    break;
                case "error":
                    HandleErrorMessage(root);
                    break;
                default:
                    Console.WriteLine($"Mensagem não tratada: {messageType}");
                    break;
            }
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage($"Erro ao processar mensagem: {ex.Message}"));
        }
    }

    private void HandleWelcomeMessage(JsonElement root)
    {
        ClientId = root.GetProperty("clientId").GetString();
        WeakReferenceMessenger.Default.Send(new StatusMessage("Conectado ao servidor!"));
    }

    private async Task HandleLoginSuccessAsync(JsonElement root)
    {
        Username = root.GetProperty("username").GetString();
        
        if (root.TryGetProperty("onlineUsers", out var usersElement))
        {
            var onlineUsers = usersElement.EnumerateArray().Select(u => u.GetString()!).ToList();
            WeakReferenceMessenger.Default.Send(new OnlineUsersMessage(onlineUsers));
        }
        
        WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(Username!));
        WeakReferenceMessenger.Default.Send(new StatusMessage($"Logado como: {Username}"));
    }

    private void HandleLoginFailed(JsonElement root)
    {
        var errorMessage = root.GetProperty("message").GetString();
        WeakReferenceMessenger.Default.Send(new ErrorMessage($"Falha no login: {errorMessage}"));
    }

    private void HandleOnlineUsers(JsonElement root)
    {
        var users = root.GetProperty("users").EnumerateArray().Select(u => u.GetString()!).ToList();
        WeakReferenceMessenger.Default.Send(new OnlineUsersMessage(users));
    }

    private void HandleUserOnline(JsonElement root)
    {
        var username = root.GetProperty("username").GetString();
        WeakReferenceMessenger.Default.Send(new UserStatusMessage(username!, true));
    }

    private void HandleUserOffline(JsonElement root)
    {
        var username = root.GetProperty("username").GetString();
        WeakReferenceMessenger.Default.Send(new UserStatusMessage(username!, false));
    }

    private void HandlePong(JsonElement root)
    {
        // Opcional: implementar health check
    }

    private void HandleErrorMessage(JsonElement root)
    {
        var errorMessage = root.GetProperty("message").GetString();
        WeakReferenceMessenger.Default.Send(new ErrorMessage($"Erro do servidor: {errorMessage}"));
    }

    public async Task SendAsync(object message)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Cliente não conectado");
        }

        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            await _writer!.WriteLineAsync(json);
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new ErrorMessage($"Erro ao enviar mensagem: {ex.Message}"));
            Disconnect();
        }
    }

    public async Task LoginAsync(string username, string password)
    {
        await SendAsync(new 
        {
            type = "login",
            username = username,
            password = password
        });
    }

    public async Task RequestOnlineUsersAsync()
    {
        await SendAsync(new 
        {
            type = "get_online_users"
        });
    }

    public async Task SendPingAsync()
    {
        await SendAsync(new 
        {
            type = "ping"
        });
    }

    public void Disconnect()
    {
        _cancellationTokenSource.Cancel();
        
        _reader?.Dispose();
        _writer?.Dispose();
        _stream?.Dispose();
        _tcpClient?.Close();
        
        _isConnected = false;
        
        WeakReferenceMessenger.Default.Send(new StatusMessage("Desconectado do servidor"));
    }

    public void Dispose()
    {
        Disconnect();
        _cancellationTokenSource.Dispose();
    }
}