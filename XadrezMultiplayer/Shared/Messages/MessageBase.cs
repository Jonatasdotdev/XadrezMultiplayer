using System.Text.Json;

namespace Shared.Messages
{
    public abstract class MessageBase
    {
        public string Type { get; set; } = null!;
        
        public static T? Deserialize<T>(string json) where T : MessageBase
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}