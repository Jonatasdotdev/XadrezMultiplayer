using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Services;
using Shared;

var builder = Host.CreateApplicationBuilder(args);

// Configurar como Windows Service
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "Xadrez Multiplayer Server";
});

// Configurar serviços
builder.Services.Configure<ServerSettings>(
    builder.Configuration.GetSection("ServerSettings"));

builder.Services.AddSingleton<GameServer>();
builder.Services.AddHostedService<WindowsGameService>();

var host = builder.Build();
await host.RunAsync();