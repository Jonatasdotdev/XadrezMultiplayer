using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Server.Services;

public class WindowsGameService : IHostedService
{
    private readonly GameServer _gameServer;
    private readonly ILogger<WindowsGameService> _logger;
    private readonly CancellationTokenSource _cts = new();

    public WindowsGameService(GameServer gameServer, ILogger<WindowsGameService> logger)
    {
        _gameServer = gameServer;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Iniciando o serviço Xadrez Multiplayer Server às {Time}", DateTime.Now);
        _ = Task.Run(() => _gameServer.StartAsync(_cts.Token), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parando o serviço Xadrez Multiplayer Server às {Time}", DateTime.Now);
        _cts.Cancel();
        return Task.CompletedTask;
    }
}