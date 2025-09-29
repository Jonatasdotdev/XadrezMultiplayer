using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using Shared;

namespace Server.Services;

public class GameServer
{
    private readonly ILogger<GameServer> _logger;
    private readonly ServerSettings _settings;
    private TcpListener _listener;
    private readonly List<ClientHandler> _clients = new();
    private readonly object _clientsLock = new();
    private readonly IServiceProvider _serviceProvider;

    public GameServer(ILogger<GameServer> logger, IOptions<ServerSettings> settings, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _settings = settings.Value;
        _serviceProvider = serviceProvider;
        _listener = new TcpListener(IPAddress.Parse(_settings.IpAddress), _settings.Port);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _listener.Start();
        _logger.LogInformation("Servidor socket iniciado em {IpAddress}:{Port} - Aguardando conexões... às {Time}", 
            _settings.IpAddress, _settings.Port, DateTime.Now);

        while (!cancellationToken.IsCancellationRequested && _clients.Count < _settings.MaxConnections)
        {
            try
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);
                var handler = _serviceProvider.GetRequiredService<ClientHandler>();
                handler.Initialize(client, this);

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
                _logger.LogError(ex, "Erro ao aceitar conexão às {Time}", DateTime.Now);
            }
        }

        _logger.LogWarning("Limite de conexões ({MaxConnections}) atingido ou servidor parado.", _settings.MaxConnections);
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
                    _logger.LogWarning(ex, "Erro no broadcast para {Username} às {Time}", client.Username, DateTime.Now);
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