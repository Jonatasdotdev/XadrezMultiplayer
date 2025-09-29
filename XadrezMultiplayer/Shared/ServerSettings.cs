namespace Shared;

public class ServerSettings
{
    public string IpAddress { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8080;
    public int MaxConnections { get; set; } = 100;
    public int ConnectionTimeout { get; set; } = 30;
}