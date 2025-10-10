using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Client.Messages
{
    public class StatusMessage(string message) : ValueChangedMessage<string>(message);
    public class ErrorMessage(string error) : ValueChangedMessage<string>(error);
    public class LoginSuccessMessage(string username) : ValueChangedMessage<string>(username);
    public class RegisterSuccessMessage(string username) : ValueChangedMessage<string>(username);
    public class RegisterFailedMessage(string error) : ValueChangedMessage<string>(error);
    public class OnlineUsersMessage(List<string> users) : ValueChangedMessage<List<string>>(users);
    public class UserStatusMessage(string username, bool isOnline) 
        : ValueChangedMessage<(string Username, bool IsOnline)>((username, isOnline));
}