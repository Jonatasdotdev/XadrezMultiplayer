using System.Text.Json;
using System.Threading.Tasks;

namespace Server.Handlers;

public interface IMessageHandler
{
    string MessageType { get; }
    Task HandleAsync(JsonElement data, ClientHandler clientHandler);
}