using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Client.Services;
using Client.Messages;
using Shared.Messages;

namespace Client.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly INetworkClient _networkClient;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _status = "Digite suas credenciais";

        public LoginViewModel(INetworkClient networkClient, IMessenger messenger)
        {
            _networkClient = networkClient;
            _messenger = messenger;
            _messenger.Register<LoginSuccessMessage>(this, (r, m) => OnLoginSuccess(m));
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Status = "Preencha todos os campos";
                return;
            }

            Status = "Conectando e fazendo login...";
            try
            {
                await _networkClient.ConnectAsync("127.0.0.1", 8080); 
                var request = new LoginRequest { Username = Username, Password = Password };
                await _networkClient.SendMessageAsync(request);
                Status = "Aguardando resposta do servidor...";
            }
            catch (Exception ex)
            {
                Status = $"Erro: {ex.Message}";
            }
        }

        private void OnLoginSuccess(LoginSuccessMessage message)
        {
            Status = $"Login bem-sucedido como {message.Value}!";
            // Navegar para GameWindow (implementar navegação)
        }
    }
}