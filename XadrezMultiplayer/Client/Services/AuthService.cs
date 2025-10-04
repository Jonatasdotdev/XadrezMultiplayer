using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace Server.Services
{
    public class AuthService
    {
        private readonly string _usersFilePath = Path.Combine(AppContext.BaseDirectory, "users.json");
        private readonly ILogger<AuthService> _logger;
        private readonly SemaphoreSlim _fileLock = new(1, 1);

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;
        }

        // Estrutura do usuário armazenado
        private class UserRecord
        {
            [JsonPropertyName("username")]
            public string Username { get; set; } = string.Empty;

            [JsonPropertyName("passwordHash")]
            public string PasswordHash { get; set; } = string.Empty;

            [JsonPropertyName("email")]
            public string Email { get; set; } = string.Empty;
        }

        public class RegisterResult
        {
            public bool IsSuccess { get; set; }
            public string? ErrorMessage { get; set; }

            public static RegisterResult Success() => new() { IsSuccess = true };
            public static RegisterResult Failed(string error) => new() { IsSuccess = false, ErrorMessage = error };
        }

        // Registrar novo usuário
        public async Task<RegisterResult> RegisterUserAsync(string username, string password, string email)
        {
            _logger.LogInformation("Caminho de gravação: {Path}", _usersFilePath);
            await _fileLock.WaitAsync();

            try
            {
                var users = await LoadUsersAsync();

                if (users.Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    return RegisterResult.Failed("Usuário já existe");
                }

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                users.Add(new UserRecord
                {
                    Username = username,
                    PasswordHash = hashedPassword,
                    Email = email
                });

                await SaveUsersAsync(users);

                _logger.LogInformation("Novo usuário registrado: {Username}", username);
                return RegisterResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário {Username}", username);
                return RegisterResult.Failed("Erro interno no servidor");
            }
            finally
            {
                _fileLock.Release();
            }
        }

        // Autenticar usuário (para login)
        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            await _fileLock.WaitAsync();

            try
            {
                var users = await LoadUsersAsync();
                var user = users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

                if (user == null)
                    return false;

                return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao validar credenciais de {Username}", username);
                return false;
            }
            finally
            {
                _fileLock.Release();
            }
        }

        private async Task<List<UserRecord>> LoadUsersAsync()
        {
            if (!File.Exists(_usersFilePath))
                return new List<UserRecord>();

            var json = await File.ReadAllTextAsync(_usersFilePath);
            return JsonSerializer.Deserialize<List<UserRecord>>(json) ?? new List<UserRecord>();
        }

        private async Task SaveUsersAsync(List<UserRecord> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_usersFilePath, json);
        }
    }
}
