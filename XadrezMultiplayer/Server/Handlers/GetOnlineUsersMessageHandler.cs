using System.Threading.Tasks;

namespace Server.Handlers;

public class GetOnlineUsersMessageHandler : IMessageHandler
{
    public string MessageType => "get_online_users";

    public async Task HandleAsync(JsonElement data, ClientHandler clientHandler)
    {
        var onlineUsers = clientHandler.Server.GetOnlineUsers();
        await clientHandler.SendMessageAsync(new {
            type = "online_users",
            users = onlineUsers
        });
    }
}