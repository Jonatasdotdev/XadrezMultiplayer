using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace Server.Services
{
    public class ClientHandler
    {
        public ClientState State { get; }
        public GameServer Server { get; }
        private readonly NetworkConnection _connection;
        private readonly MessageProcessor _messageProcessor;
        private readonly HeartbeatManager _heartbeatManager;
        private readonly ILogger<ClientHandler> _logger;
        private readonly ILogger<MessageProcessor> _messageProcessorLogger;
        private readonly ILogger<HeartbeatManager> _heartbeatLogger;
        private readonly AuthService _authService;
        private readonly GameSessionManager _gameSessionManager;
        public ClientHandler(TcpClient client, GameServer server, ILogger<ClientHandler> logger,
                            ILogger<MessageProcessor> messageProcessorLogger,
                            ILogger<HeartbeatManager> heartbeatLogger,
                            AuthService authService, GameSessionManager gameSessionManager,
                            IOptions<ServerSettings> settings, IEnumerable<IMessageHandler> messageHandlers)
        {
            State = new ClientState();
            Server = server;
            _logger = logger;
            _messageProcessorLogger = messageProcessorLogger;
            _heartbeatLogger = heartbeatLogger;
            _authService = authService;
            _gameSessionManager = gameSessionManager;
            _connection = new NetworkConnection(client, logger, settings);
            _heartbeatManager = new HeartbeatManager(_heartbeatLogger);
            _messageProcessor = new MessageProcessor(messageHandlers, _messageProcessorLogger);
        }
        

        public async Task HandleClientAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cliente conectado: {ClientId}", State.ClientId);

                await SendMessageAsync(new
                {
                    type = "welcome",
                    clientId = State.ClientId,
                    message = "Conectado ao servidor de Xadrez",
                    serverVersion = "1.0.0"
                });

                _ = Task.Run(() => _heartbeatManager.RunAsync(State, _connection, State.ClientId, cancellationToken), cancellationToken);

                string? message;
                while (!cancellationToken.IsCancellationRequested &&
                       (message = await _connection.ReadMessageWithTimeoutAsync(cancellationToken)) != null)
                {
                    State.LastActivity = DateTime.UtcNow;
                    _logger.LogDebug("Mensagem de {ClientId}: {Message}", State.ClientId, message);
                    await _messageProcessor.ProcessAsync(message, this);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cliente {ClientId} desconectado (cancellation)", State.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro com cliente {ClientId}", State.ClientId);
            }
            finally
            {
                await HandleDisconnectionAsync();
            }
        }

        public async Task SendMessageAsync(object message)
        {
            await _connection.SendMessageAsync(message);
        }

        internal async Task HandleGameEndAsync(MoveResult moveResult)
        {
            if (State.CurrentGame == null) return;

            var gameEndMessage = new
            {
                type = "game_ended",
                reason = moveResult.IsCheckmate ? "checkmate" : "draw",
                winner = moveResult.IsCheckmate ? State.Username : null,
                isDraw = moveResult.IsDraw,
                finalBoard = State.CurrentGame.GetBoardState()
            };

            await State.CurrentGame.BroadcastToPlayers(gameEndMessage);

            if (moveResult.IsCheckmate)
            {
                await _authService.UpdatePlayerStats(State.Username!, true);
                var opponentUsername = State.CurrentGame.GetOpponent(this)?.State.Username;
                if (opponentUsername != null)
                    await _authService.UpdatePlayerStats(opponentUsername, false);
            }

            State.CurrentGame = null;
        }

        private async Task HandleDisconnectionAsync()
        {
            try
            {
                if (State.CurrentGame != null)
                {
                    var opponent = State.CurrentGame.GetOpponent(this);
                    if (opponent != null)
                    {
                        await opponent.SendMessageAsync(new
                        {
                            type = "opponent_disconnected",
                            message = $"{State.Username} desconectou-se"
                        });
                    }

                    State.CurrentGame.HandlePlayerDisconnect(this);
                    State.CurrentGame = null;
                }

                Server.RemoveClient(this);

                if (!string.IsNullOrEmpty(State.Username))
                {
                    Server.BroadcastMessage(JsonSerializer.Serialize(new
                    {
                        type = "user_offline",
                        username = State.Username
                    }), this);
                }

                _logger.LogInformation("Cliente desconectado: {ClientId} ({Username})", State.ClientId, State.Username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante desconexão de {ClientId}", State.ClientId);
            }
            finally
            {
                _connection.Dispose();
            }
        }
    }
}