namespace Shared.Messages
{
    public class LoginRequest : MessageBase
    {
        public LoginRequest() => Type = "login";
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginResponse : MessageBase
    {
        public LoginResponse() => Type = "login_response";
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Username { get; set; }

       
    }

    public class RegisterRequest : MessageBase
    {
        public RegisterRequest() => Type = "register";
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        
        public string Email { get; set; } = string.Empty;
    }

    public class RegisterResponse : MessageBase
    {
        public RegisterResponse() => Type = "register_response";
        public bool Success { get; set; }
        public string? Error { get; set; }
    }
}