using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Server.Models;

namespace Server.Services;

public class AuthService
{
    private readonly Dictionary<string, User> _users = new();
    private readonly object _lock = new object();

    public async Task<AuthResult> AuthenticateUserAsync(string username, string password)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_users.TryGetValue(username, out var user))
                {
                    if (VerifyPassword(password, user.PasswordHash, user.Salt))
                    {
                        return AuthResult.Success();
                    }
                }
                return AuthResult.Failure("Credenciais inválidas");
            }
        });
    }

    public async Task<AuthResult> RegisterUserAsync(string username, string password, string email)
    {
        return await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_users.ContainsKey(username))
                {
                    return AuthResult.Failure("Usuário já existe");
                }

                var salt = GenerateSalt();
                var passwordHash = HashPassword(password, salt);

                var user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    GamesWon = 0,
                    GamesLost = 0
                };

                _users[username] = user;
                return AuthResult.Success();
            }
        });
    }

    public async Task UpdatePlayerStats(string username, bool won)
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                if (_users.TryGetValue(username, out var user))
                {
                    if (won)
                        user.GamesWon++;
                    else
                        user.GamesLost++;
                }
            }
        });
    }

    private string HashPassword(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var saltedPassword = password + salt;
        var bytes = Encoding.UTF8.GetBytes(saltedPassword);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private string GenerateSalt()
    {
        var bytes = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private bool VerifyPassword(string password, string storedHash, string salt)
    {
        var computedHash = HashPassword(password, salt);
        return storedHash == computedHash;
    }
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}