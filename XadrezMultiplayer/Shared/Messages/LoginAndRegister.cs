namespace Shared.Messages
{

    public class RegisterRequest : MessageBase
    {
        public RegisterRequest() => Type = "register";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class RegisterResponse : MessageBase
    {
        public RegisterResponse() => Type = "register_response";
        public bool Success { get; set; }
        public string? Username { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class LoginRequest : MessageBase
    {
        public LoginRequest() => Type = "login";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse : MessageBase
    {
        public LoginResponse() => Type = "login_response";
        public bool Success { get; set; }
        public string? Username { get; set; }
        public string? ErrorMessage { get; set; }
    }
}