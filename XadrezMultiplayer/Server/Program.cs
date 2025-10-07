using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Services;
using Server.Handlers;
using Shared;

namespace Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            builder.Services.AddWindowsService(options => { options.ServiceName = "Xadrez Multiplayer Server"; });

            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Information);
            });

            builder.Services.Configure<ServerSettings>(
                builder.Configuration.GetSection("ServerSettings"));

            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<GameSessionManager>();
            builder.Services.AddScoped<GameServer>();

            builder.Services.AddTransient<ClientState>();
            builder.Services.AddTransient<HeartbeatManager>();
            builder.Services.AddTransient<MessageProcessor>();

            builder.Services.AddTransient<IMessageHandler, LoginMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, RegisterMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, PingMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, GetOnlineUsersMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, InvitePlayerMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, AcceptInviteMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, RejectInviteMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, MakeMoveMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, ResignGameMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, OfferDrawMessageHandler>();
            builder.Services.AddTransient<IMessageHandler, RespondDrawMessageHandler>();

            builder.Services.AddDbContext<ChessDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddHostedService(provider =>
            {
                var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
                var scope = scopeFactory.CreateScope();
                var gameServer = scope.ServiceProvider.GetRequiredService<GameServer>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<GameServerHostedService>>();
                return new GameServerHostedService(gameServer, logger, scope);
            });

            var host = builder.Build();
            await host.RunAsync();
        }

        // Implementação personalizada do IHostedService para rodar o GameServer
        public class GameServerHostedService : BackgroundService, IDisposable
        {
            private readonly GameServer _gameServer;
            private readonly ILogger<GameServerHostedService> _logger;
            private readonly IServiceScope _scope;
            private readonly CancellationTokenSource _cts = new();

            public GameServerHostedService(GameServer gameServer, ILogger<GameServerHostedService> logger,
                IServiceScope scope)
            {
                _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _scope = scope ?? throw new ArgumentNullException(nameof(scope));
            }

            protected override async Task ExecuteAsync(CancellationToken stoppingToken)
            {
                try
                {
                    _logger.LogInformation("Iniciando GameServer às {Time}", DateTime.Now.ToString("HH:mm:ss"));
                    await _gameServer.StartAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao iniciar o GameServer às {Time}", DateTime.Now.ToString("HH:mm:ss"));
                }
            }

            public override async Task StopAsync(CancellationToken cancellationToken)
            {
                _logger.LogInformation("Parando GameServer às {Time}", DateTime.Now.ToString("HH:mm:ss"));
                _cts.Cancel();
                await base.StopAsync(cancellationToken);
            }

            public void Dispose()
            {
                _scope.Dispose();
                _cts.Dispose();
            }
        }
    }
}
