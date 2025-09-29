using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace Server.Services;

public class GameServer
{
    private readonly ILogger<GameServer> _logger;
    private readonly ServerSettings _settings;
    private TcpListener _listener;
    private readonly List<ClientHandler> _clients = new();
    private readonly object _clientsLock = new();

    public GameServer(ILogger<GameServer> logger, IOptions<ServerSettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
        _listener = new TcpListener(IPAddress.Parse(_settings.IpAddress), _settings.Port);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        _logger.LogInformation("Servidor socket iniciado - Aguardando conexões...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                var handler = new ClientHandler(client, this, _logger);
                
                lock (_clientsLock)
                {
                    _clients.Add(handler);
                }
                
                _ = Task.Run(() => handler.HandleClientAsync(cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao aceitar conexão");
            }
        }
    }

    public void BroadcastMessage(string message, ClientHandler? excludeClient = null)
    {
        List<ClientHandler> clientsCopy;
        
        lock (_clientsLock)
        {
            clientsCopy = new List<ClientHandler>(_clients);
        }

        foreach (var client in clientsCopy.Where(c => c != excludeClient && c.IsAuthenticated))
        {
            _ = Task.Run(async () => 
            {
                try
                {
                    await client.SendMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erro no broadcast para {Username}", client.Username);
                }
            });
        }
    }

    public ClientHandler? GetClientByUsername(string username)
    {
        lock (_clientsLock)
        {
            return _clients.FirstOrDefault(c => 
                c.IsAuthenticated && c.Username == username);
        }
    }

    public void RemoveClient(ClientHandler client)
    {
        lock (_clientsLock)
        {
            _clients.Remove(client);
        }
    }

    public List<string> GetOnlineUsers()
    {
        lock (_clientsLock)
        {
            return _clients
                .Where(c => c.IsAuthenticated)
                .Select(c => c.Username!)
                .ToList();
        }
    }
}