using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Client.Messages;
using Client.Services;
using Shared.Messages;
using System.Threading.Tasks;
using System.Windows;

namespace Client.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly INetworkClient _networkClient;
        private readonly IMessenger _messenger;
        private readonly DialogService _dialogService;

        [ObservableProperty] private string _username = string.Empty;

        [ObservableProperty] private string _password = string.Empty;

        [ObservableProperty] private string _email = string.Empty;

        [ObservableProperty] private string _status = "Preencha os dados para registrar-se";

        public RegisterViewModel(INetworkClient networkClient, IMessenger messenger, DialogService dialogService)
        {
            _networkClient = networkClient ?? throw new ArgumentNullException(nameof(networkClient));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _messenger.Register<RegisterSuccessMessage>(this, (r, m) => OnRegisterSuccess(m));
            _messenger.Register<RegisterFailedMessage>(this, (r, m) => OnRegisterFailed(m));
        }

       

        [RelayCommand]
        private async Task RegisterAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email))
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

        private void OnRegisterSuccess(RegisterSuccessMessage message)
        {
            Status = $"Registro bem-sucedido como {message.Value}! Redirecionando para login...";
            _dialogService.CloseCurrentDialog(Application.Current.MainWindow as Window);
            _dialogService.ShowLoginDialog();
        }

        private void OnRegisterFailed(RegisterFailedMessage message)
        {
            Status = $"Erro no registro: {message.Value}";


            [RelayCommand]
            void BackToLogin()
            {
                _dialogService.CloseCurrentDialog(Application.Current.MainWindow as Window);
                _dialogService.ShowLoginDialog();
            }
        }
    }
}