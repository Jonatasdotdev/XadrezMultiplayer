using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Client.Messages;
using Client.Services;
using Shared.Messages;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly INetworkClient _networkClient;
        private readonly IMessenger _messenger;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _status = "Preencha os dados para registrar-se";

        public RegisterViewModel(INetworkClient networkClient, IMessenger messenger)
        {
            _networkClient = networkClient;
            _messenger = messenger;
            _messenger.Register<RegisterResponse>(this, (r, m) => OnRegisterResponse(m));
        }

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Email))
            {
                Status = "Preencha todos os campos";
                return;
            }

            Status = "Conectando e registrando...";
            try
            {
                await _networkClient.ConnectAsync("127.0.0.1", 8080);
                var request = new RegisterRequest
                {
                    Username = Username,
                    Password = Password,
                    Email = Email
                };
                await _networkClient.SendMessageAsync(request); 
                Status = "Aguardando resposta do servidor...";
            }
            catch (Exception ex)
            {
                Status = $"Erro: {ex.Message}";
            }
        }

        private void OnRegisterResponse(RegisterResponse response)
        {
            if (response.Success)
            {
                Status = $"Registro bem-sucedido como {Username}! Redirecionando para login...";
                // Navegar de volta para LoginDialog (implementar navegação)
            }
            else
            {
                Status = $"Erro no registro: {response.Error ?? "Erro desconhecido"}";
            }
        }

        [RelayCommand]
        private void BackToLogin()
        {
            // Implementar navegação para LoginDialog
            Status = "Voltando ao login...";
        }
    }
}