using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Client.Services;
using CommunityToolkit.Mvvm.Messaging;
using Client.Models;
using Client.ViewModels;
using Client.Views;

namespace Client
{
    public partial class App : Application
    {
        private IHost? _host;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
                    services.AddSingleton<INetworkClient, NetworkClient>();
                    services.Configure<NetworkSettings>(settings =>
                    {
                        settings.DefaultIp = "127.0.0.1";
                        settings.DefaultPort = 8080;
                    });

                    // Register Dialogs and Service
                    services.AddTransient<LoginDialog>();
                    services.AddTransient<RegisterDialog>();
                    services.AddSingleton<DialogService>();

                    // Register ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<GameViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                })
                .Build();

            _host.Start();

            // Expor os serviços via Application.Current
            Current.Properties["Host"] = _host;

            // Mostrar LoginDialog ao iniciar
            var dialogService = _host.Services.GetService<DialogService>();
            dialogService?.ShowLoginDialog();
        }

        public static IServiceProvider Services => (Current.Properties["Host"] as IHost)?.Services ?? throw new InvalidOperationException("Host não inicializado");

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}