using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Server.Services;

public class HeartbeatManager
{
    private readonly ILogger _logger;

    public HeartbeatManager(ILogger logger)
    {
        _logger = logger;
    }

    public async Task RunAsync(ClientState state, NetworkConnection connection, string clientId, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && connection.IsConnected())
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                
                if ((DateTime.UtcNow - state.LastActivity) > TimeSpan.FromMinutes(2))
                {
                    _logger.LogWarning("Heartbeat falhou para {ClientId}", clientId);
                    break;
                }

                if (connection.IsConnected())
                {
                    await connection.SendMessageAsync(new { type = "ping", timestamp = DateTime.UtcNow });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro no heartbeat do cliente {ClientId}", clientId);
                break;
            }
        }
    }
}