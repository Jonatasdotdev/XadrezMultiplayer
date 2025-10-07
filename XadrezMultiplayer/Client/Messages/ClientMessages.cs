using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Messages

{
    public class RegisterSuccessMessage(string username) : ValueChangedMessage<string>(username);
    public class RegisterFailedMessage(string error) : ValueChangedMessage<string>(error);

// Mensagem de status geral
    public class StatusMessage(string message) : ValueChangedMessage<string>(message);

    public class ErrorMessage(string error) : ValueChangedMessage<string>(error);

// Mensagem de login
    public class LoginSuccessMessage(string username) : ValueChangedMessage<string>(username);

// Mensagem de usuários online
    public class OnlineUsersMessage(List<string> users) : ValueChangedMessage<List<string>>(users);

// Mensagem de status de usuário específico
    public class UserStatusMessage(string username, bool isOnline)
        : ValueChangedMessage<(string Username, bool IsOnline)>((username, isOnline));
}