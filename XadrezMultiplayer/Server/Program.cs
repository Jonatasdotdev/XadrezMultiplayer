using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Services;
using Server.Handlers;
using Shared;

namespace Server;

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

        // Configurar serviços
        builder.Services.Configure<ServerSettings>(
            builder.Configuration.GetSection("ServerSettings"));

        // Registrar serviços como singletons (estado global)
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<GameSessionManager>();
        builder.Services.AddSingleton<GameServer>();

        // Registrar serviços como transient (nova instância por uso)
        builder.Services.AddTransient<ClientHandler>();
        builder.Services.AddTransient<ClientState>();
        builder.Services.AddTransient<NetworkConnection>();
        builder.Services.AddTransient<HeartbeatManager>();
        builder.Services.AddTransient<MessageProcessor>();

        // Registrar todos os IMessageHandler (transient)
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

        // Adicionar o serviço hospedado para iniciar o GameServer
        builder.Services.AddHostedService<WindowsGameService>();

        var host = builder.Build();
        await host.RunAsync();
    }
}