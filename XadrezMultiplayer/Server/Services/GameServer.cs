using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Shared;

namespace Server.Services
{
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
                _settings.IpAddress, _settings.Port, DateTime.Now.ToString("HH:mm:ss"));

            while (!cancellationToken.IsCancellationRequested && _clients.Count < _settings.MaxConnections)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync(cancellationToken);

                    // Resolver dependências via DI
                    using var scope = _serviceProvider.CreateScope();
                    var scopeProvider = scope.ServiceProvider;

                    var logger = scopeProvider.GetRequiredService<ILogger<ClientHandler>>();
                    var messageProcessorLogger = scopeProvider.GetRequiredService<ILogger<MessageProcessor>>();
                    var heartbeatLogger = scopeProvider.GetRequiredService<ILogger<HeartbeatManager>>();
                    var authService = scopeProvider.GetRequiredService<AuthService>();
                    var gameSessionManager = scopeProvider.GetRequiredService<GameSessionManager>();
                    var settings = scopeProvider.GetRequiredService<IOptions<ServerSettings>>();
                    var messageHandlers = scopeProvider.GetServices<IMessageHandler>();

                    // Instanciar ClientHandler manualmente com o TcpClient e as dependências resolvidas
                    var handler = new ClientHandler(client, this, logger, messageProcessorLogger, heartbeatLogger, authService, gameSessionManager, settings, messageHandlers);

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
                    _logger.LogError(ex, "Erro ao aceitar conexão às {Time}", DateTime.Now.ToString("HH:mm:ss"));
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

            foreach (var client in clientsCopy.Where(c => c.State.IsAuthenticated))
            {
                if (client != excludeClient)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await client.SendMessageAsync(message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Erro no broadcast para {Username} às {Time}", client.State.Username, DateTime.Now.ToString("HH:mm:ss"));
                        }
                    });
                }
            }
        }

        public ClientHandler? GetClientByUsername(string username)
        {
            lock (_clientsLock)
            {
                return _clients.FirstOrDefault(c =>
                    c.State.IsAuthenticated && c.State.Username == username);
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
                    .Where(c => c.State.IsAuthenticated)
                    .Select(c => c.State.Username!)
                    .ToList();
            }
        }
    }
}