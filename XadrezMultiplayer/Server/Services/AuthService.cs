using Microsoft.EntityFrameworkCore;
using Server.Models;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Services
{
    public class AuthService
    {
        private readonly ChessDbContext _context;

        public AuthService(ChessDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResult> AuthenticateUserAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null && VerifyPassword(password, user.PasswordHash, user.Salt))
            {
                user.CreatedAt = DateTime.UtcNow; // Atualiza último login
                await _context.SaveChangesAsync();
                return AuthResult.Success();
            }
            return AuthResult.Failure("Credenciais inválidas");
        }

        public async Task<AuthResult> RegisterUserAsync(string username, string password, string email)
        {
            if (await _context.Users.AnyAsync(u => u.Username == username))
                return AuthResult.Failure("Usuário já existe");

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

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return AuthResult.Success();
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
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public static AuthResult Success() => new() { IsSuccess = true };
    public static AuthResult Failure(string error) => new() { IsSuccess = false, ErrorMessage = error };
}