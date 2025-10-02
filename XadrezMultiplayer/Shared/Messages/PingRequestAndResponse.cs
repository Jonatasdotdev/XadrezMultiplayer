namespace Shared.Messages
{
  
    public class PingRequest : MessageBase
    {
        public PingRequest() => Type = MessageTypes.Ping;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

      public class PingResponse : MessageBase
    {
        public PingResponse() => Type = MessageTypes.PingResponse;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

   
 
}