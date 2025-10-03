namespace Client.Models
{
    public class NetworkSettings
    {
        public string DefaultIp { get; set; } = "127.0.0.1";
        public int DefaultPort { get; set; } = 5000;
        public int ReconnectAttempts { get; set; } = 3;
        public int ReconnectDelay { get; set; } = 5000;
        public int HeartbeatInterval { get; set; } = 30000;
    }
}