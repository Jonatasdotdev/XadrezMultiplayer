using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Client.Services;
using Client.Messages;

namespace Client.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NetworkClient _networkClient;
    
    [ObservableProperty]
    private bool _isConnected = false;

    [ObservableProperty]
    private string _serverIp = "127.0.0.1";

    [ObservableProperty]
    private int _serverPort = 8080;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private List<string> _onlineUsers = new();

    public MainViewModel(NetworkClient networkClient)
    {
        _networkClient = networkClient;
        
        // Registrar para receber mensagens
        WeakReferenceMessenger.Default.Register<StatusMessage>(this, (r, m) =>
        {
            SetStatus(m.Value);
        });
        
        WeakReferenceMessenger.Default.Register<ErrorMessage>(this, (r, m) =>
        {
            SetStatus($"Erro: {m.Value}");
        });
        
        WeakReferenceMessenger.Default.Register<LoginSuccessMessage>(this, (r, m) =>
        {
            Username = m.Value;
            IsConnected = true;
        });
        
        WeakReferenceMessenger.Default.Register<OnlineUsersMessage>(this, (r, m) =>
        {
            OnlineUsers = m.Value;
        });
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
            var connected = await _networkClient.ConnectAsync(ServerIp, ServerPort);
            if (!connected)
            {
                SetStatus("Falha na conexão");
                return;
            }

            SetStatus("Conectado! Faça login...");
        }
        catch (Exception ex)
        {
            SetStatus($"Erro: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            SetStatus("Digite um username");
            return;
        }

        IsBusy = true;
        SetStatus("Fazendo login...");

        try
        {
            await _networkClient.LoginAsync(Username, "password"); // TODO: Adicionar campo de password
            SetStatus("Login enviado...");
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
        _networkClient.Disconnect();
        IsConnected = false;
        Username = string.Empty;
        OnlineUsers.Clear();
        SetStatus("Desconectado");
    }

    [RelayCommand]
    private async Task RefreshUsersAsync()
    {
        if (!IsConnected) return;
        
        try
        {
            await _networkClient.RequestOnlineUsersAsync();
        }
        catch (Exception ex)
        {
            SetStatus($"Erro ao atualizar usuários: {ex.Message}");
        }
    }
}