using System.Net;
using System.Net.Sockets;

namespace Server;
public class GameServer
{
    private TcpListener _listener;
    private List<ClientHandler> _clients = new();

    public GameServer(string ip, int port)
    {
        _listener = new TcpListener(IPAddress.Parse(ip), port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        Console.WriteLine("Servidor iniciado...");

        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            var handler = new ClientHandler(client, this);
            _clients.Add(handler);
            _ = Task.Run(() => handler.HandleClientAsync());
        }
    }

    public void BroadcastMessage(string message, ClientHandler excludeClient = null)
    {
        foreach (var client in _clients.Where(c => c != excludeClient))
        {
            client.SendMessage(message);
        }
    }
}