using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Client.Services;
using Client.Messages;
using System.Windows;
using Shared.Messages;
using Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Client.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private readonly INetworkClient _networkClient;
        private readonly IMessenger _messenger;
        private LoginViewModel? _loginViewModel; 

        [ObservableProperty]
        private bool _isConnected = false;

        [ObservableProperty]
        private string _serverIp = "127.0.0.1";

        [ObservableProperty]
        private int _serverPort = 8080;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty; 
        [ObservableProperty]
        private List<string> _onlineUsers = new();

        [ObservableProperty]
        private bool _isBusy = false; 

        [ObservableProperty]
        private string _status = "Pronto para conectar";

        public MainViewModel(INetworkClient networkClient, IMessenger messenger)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));

            // Registrar para receber mensagens
            _messenger.Register<StatusMessage>(this, (r, m) => SetStatus(m.Value));
            _messenger.Register<ErrorMessage>(this, (r, m) => SetStatus($"Erro: {m.Value}"));
            _messenger.Register<LoginSuccessMessage>(this, (r, m) =>
            {
                Username = m.Value;
                IsConnected = true;
                OpenGameWindow();
            });
            _messenger.Register<OnlineUsersMessage>(this, (r, m) => OnlineUsers = m.Value);
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            if (IsConnected)
            {
                SetStatus("Já conectado");
                return;
            }

            IsBusy = true;
            SetStatus("Conectando...");
            try
            {
                await _networkClient.ConnectAsync(ServerIp, ServerPort);
                SetStatus("Conectado! Abrindo janela de login...");
                OpenLoginDialog();
            }
            catch (Exception ex)
            {
                SetStatus($"Erro ao conectar: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                SetStatus("Digite username e senha");
                return;
            }

            IsBusy = true;
            SetStatus("Fazendo login...");
            try
            {
                var request = new LoginRequest { Username = Username, Password = Password };
                await _networkClient.SendMessageAsync(request);
                SetStatus("Login enviado, aguardando resposta...");
            }
            catch (Exception ex)
            {
                SetStatus($"Erro no login: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void Disconnect()
        {
            _networkClient.DisconnectAsync().Wait(); 
            IsConnected = false;
            Username = string.Empty;
            Password = string.Empty;
            OnlineUsers.Clear();
            SetStatus("Desconectado");
        }

        [RelayCommand]
        private async Task RefreshUsersAsync()
        {
            if (!IsConnected) return;

            IsBusy = true;
            SetStatus("Atualizando lista de usuários...");
            try
            {
                var request = new GetOnlineUsersRequest();
                await _networkClient.SendMessageAsync(request);
                SetStatus("Lista de usuários atualizada");
            }
            catch (Exception ex)
            {
                SetStatus($"Erro ao atualizar usuários: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenLoginDialog()
        {
            if (_loginViewModel == null)
            {
                _loginViewModel = App.Services.GetService<LoginViewModel>() ?? new LoginViewModel(_networkClient, _messenger);
            }

            var loginWindow = new LoginDialog { DataContext = _loginViewModel };
            loginWindow.ShowDialog();
        }

        private void OpenGameWindow()
        {
            var gameWindow = new GameWindow
            {
                DataContext = App.Services.GetService<GameViewModel>() ?? new GameViewModel(_networkClient, _messenger)
            };
            gameWindow.Show();
            //fechar MainWindow ou LoginDialog
            Application.Current.MainWindow?.Close();
        }

        private new void SetStatus(string message)
        {
            Status = $"{message} às {DateTime.Now:HH:mm:ss}";
        }
    }
}