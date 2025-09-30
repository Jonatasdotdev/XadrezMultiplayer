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

            // Carregar configuração do appsettings.json
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            // Configurar como Windows Service
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "Xadrez Multiplayer Server";
            });

            // Configurar logging com suporte a ILogger<T>
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole(); 
                logging.AddDebug();  
                logging.SetMinimumLevel(LogLevel.Information); // Ajustavel
            });

            // Configurar configurações do servidor
            builder.Services.Configure<ServerSettings>(
                builder.Configuration.GetSection("ServerSettings"));

            // Registrar serviços como singletons (estado global)
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<GameSessionManager>();
            builder.Services.AddSingleton<GameServer>();

            // Registrar serviços como transient (nova instância por uso)
            builder.Services.AddTransient<ClientState>();
            //builder.Services.AddTransient<NetworkConnection>();
            builder.Services.AddTransient<HeartbeatManager>();
            builder.Services.AddTransient<MessageProcessor>();

            // Registrar todos os IMessageHandler (transient) com ILogger<T> implícito
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

            // Registrar serviço hospedado para iniciar o GameServer
            builder.Services.AddHostedService(provider =>
            {
                var gameServer = provider.GetRequiredService<GameServer>();
                return new GameServerHostedService(gameServer, provider.GetRequiredService<ILogger<GameServerHostedService>>());
            });

            var host = builder.Build();

            // Iniciar o host
            await host.RunAsync();
        }
    }

    // Implementação personalizada de IHostedService para iniciar o GameServer
    public class GameServerHostedService : BackgroundService
    {
        private readonly GameServer _gameServer;
        private readonly ILogger<GameServerHostedService> _logger;
        private readonly CancellationTokenSource _cts = new();

        public GameServerHostedService(GameServer gameServer, ILogger<GameServerHostedService> logger)
        {
            _gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    }
}