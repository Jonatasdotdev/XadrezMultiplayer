using CommunityToolkit.Mvvm.Messaging;
using Client.Messages;

namespace Client.Services;

public class GameService
{
    private readonly NetworkClient _networkClient;

    public GameService(NetworkClient networkClient)
    {
        _networkClient = networkClient;
    }

    // TODO: Implementar lógica de jogo
}