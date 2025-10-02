using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Options;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shared.Messages;
using Client.Messages;
using Client.Models;
using Shared;


namespace Client.Services
{
    public interface INetworkClient
    {
        bool IsConnected { get; }
    Task ConnectAsync(string ip, int port);
    Task DisconnectAsync();
    Task SendMessageAsync<T>(T message) where T : MessageBase;
    }

    public class NetworkClient : INetworkClient, IAsyncDisposable
    {
        private readonly IMessenger _messenger;
        private readonly NetworkSettings _settings;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;
        private bool _isReconnecting;
        private Task? _receiveTask;

        public bool IsConnected => _client?.Connected ?? false;

        public NetworkClient(IMessenger messenger, IOptions<NetworkSettings> settings)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task ConnectAsync(string ip, int port)
        {
            if (IsConnected) return;

            _cts = new CancellationTokenSource();
            var attempts = 0;

            while (attempts < _settings.ReconnectAttempts && !_isReconnecting)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync(ip, port, _cts.Token);
                    _stream = _client.GetStream();
                    _isReconnecting = false;
                    _receiveTask = StartReceivingAsync(_cts.Token);
                    await StartHeartbeatAsync(_cts.Token);
                    return;
                }
                catch (Exception ex)
                {
                    attempts++;
                    if (attempts == _settings.ReconnectAttempts)
                    {
                        _messenger.Send(new ErrorMessage($"Falha ao conectar: {ex.Message}"));
                        throw;
                    }
                    await Task.Delay(_settings.ReconnectDelay, _cts.Token);
                }
            }
        }

        public async Task DisconnectAsync()
        {
            _cts?.Cancel();
            _isReconnecting = false;

            if (_stream != null)
            {
                await _stream.DisposeAsync();
                _stream = null;
            }

            _client?.Close();
            _client = null;
            _receiveTask = null;
        }

        public async Task SendMessageAsync<T>(T message) where T : MessageBase
        {
            if (!IsConnected || _stream == null) throw new InvalidOperationException("Não conectado ao servidor.");

            var json = message.Serialize();
            var bytes = Encoding.UTF8.GetBytes(json + "\n");
            await _stream.WriteAsync(bytes, 0, bytes.Length, _cts?.Token ?? CancellationToken.None);
        }

        private async Task StartReceivingAsync(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            try
            {
                while (!cancellationToken.IsCancellationRequested && _client?.Connected == true)
                {
                    var bytesRead = await _stream!.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break; // Conexão fechada pelo servidor

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ProcessMessage(message);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancelamento normal
            }
            catch (Exception ex)
            {
                _messenger.Send(new ErrorMessage($"Erro na recepção: {ex.Message}"));
                if (!_isReconnecting) await AttemptReconnectAsync();
            }
        }

        private void ProcessMessage(string message)
        {
            //Processar diferentes tipos de mensagens
            var response = MessageBase.Deserialize<LoginResponse>(message);
            if (response?.Type == MessageTypes.LoginResponse && response.Success)
            {
                _messenger.Send(new LoginSuccessMessage(response.Username ?? string.Empty));
            }
            else if (MessageBase.Deserialize<GetOnlineUsersResponse>(message) is GetOnlineUsersResponse usersResponse)
            {
                _messenger.Send(new OnlineUsersMessage(usersResponse.Users));
            }
            // Adicionar mais casos conforme necessário (MoveResponse, InviteResponse)
        }

        private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                await Task.Delay(_settings.HeartbeatInterval, cancellationToken);
                if (IsConnected)
                {
                    await SendMessageAsync(new PingRequest());
                }
            }
        }

        private async Task AttemptReconnectAsync()
        {
            if (_isReconnecting || _cts?.IsCancellationRequested == true) return;

            _isReconnecting = true;
            _messenger.Send(new StatusMessage("Tentando reconectar..."));
            await Task.Delay(_settings.ReconnectDelay);

            try
            {
                await ConnectAsync(_settings.DefaultIp, _settings.DefaultPort);
                _messenger.Send(new StatusMessage("Reconectado com sucesso!"));
            }
            catch
            {
                _messenger.Send(new ErrorMessage("Falha na reconexão."));
                _isReconnecting = false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _cts?.Dispose();
        }
    }
}