using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Models;
using Server.Handlers;
using Shared;

namespace Server.Services;

public class NetworkConnection : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly StreamReader _reader;
    private readonly StreamWriter _writer;
    private readonly ILogger _logger;
    private readonly int _timeoutSeconds;

   public NetworkConnection(TcpClient client, ILogger logger, IOptions<ServerSettings> settings)
    {
        _client = client;
        _logger = logger;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, Encoding.UTF8);
        _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
        _timeoutSeconds = settings.Value.ConnectionTimeout;
    }

    public async Task<string?> ReadMessageWithTimeoutAsync(CancellationToken cancellationToken)
{
    try
    {
        var readTask = _reader.ReadLineAsync();
        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_timeoutSeconds), cancellationToken);

        var completedTask = await Task.WhenAny(readTask, timeoutTask);
        
        if (completedTask == timeoutTask)
        {
            await SendMessageAsync(new { type = "timeout_warning", message = "Conexão ociosa por muito tempo" });
            return null;
        }

        return await readTask;
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Timeout/erro na leitura do cliente");
        return null;
    }
}

    public async Task SendMessageAsync(object message)
    {
        if (!IsConnected()) return;

        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            await _writer.WriteLineAsync(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar mensagem");
        }
    }

    public bool IsConnected()
    {
        return _client?.Connected == true && 
               _stream?.CanWrite == true && 
               _stream?.CanRead == true;
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _stream?.Dispose();
        _client?.Close();
    }
}