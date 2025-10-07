using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using static BCrypt.Net.BCrypt;

namespace Server.Services
{
    public class AuthService
    {
        private readonly ChessDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ChessDbContext context, ILogger<AuthService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResult> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                user.CreatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuário {Username} autenticado com sucesso", username);
                return AuthResult.Success();
            }
            _logger.LogWarning("Falha na autenticação para {Username}", username);
            return AuthResult.Failure("Credenciais inválidas");
        }

        public async Task<AuthResult> RegisterUserAsync(string username, string password, string email)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == username))
                {
                    _logger.LogWarning("Tentativa de registro com username existente: {Username}", username);
                    return AuthResult.Failure("Usuário já existe");
                }

                if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(email))
                {
                    return AuthResult.Failure("Password e email são obrigatórios");
                }

                var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
                var user = new User
                {
                    Username = username,
                    PasswordHash = passwordHash,
                    Salt = string.Empty, // BCrypt gerencia o salt internamente
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    GamesWon = 0,
                    GamesLost = 0
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Usuário {Username} registrado com sucesso", username);
                return AuthResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar usuário {Username}", username);
                return AuthResult.Failure("Erro durante registro");
            }
        }

        public async Task UpdatePlayerStats(string username, bool won)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null)
            {
                if (won) user.GamesWon++;
                else user.GamesLost++;
                await _context.SaveChangesAsync();
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}}