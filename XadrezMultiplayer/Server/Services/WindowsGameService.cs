using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared;

namespace Server.Services;

public class WindowsGameService : BackgroundService
{
    private readonly GameServer _gameServer;
    private readonly ILogger<WindowsGameService> _logger;
    private readonly ServerSettings _settings;

    public WindowsGameService(
        GameServer gameServer, 
        ILogger<WindowsGameService> logger,
        IOptions<ServerSettings> settings)
    {
        _gameServer = gameServer;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço Xadrez Multiplayer iniciando...");
        _logger.LogInformation("Endpoint: {Ip}:{Port}", _settings.IpAddress, _settings.Port);

        try
        {
            await _gameServer.StartAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Serviço cancelado via token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro fatal no servidor de xadrez");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Serviço Xadrez Multiplayer parando...");
        await base.StopAsync(cancellationToken);
    }
}