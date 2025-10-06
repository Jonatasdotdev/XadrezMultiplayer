using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Server.Models
{
    public class ChessDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GameInvite> GameInvites { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Log> Logs { get; set; }

        private readonly IConfiguration _configuration;

        public ChessDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<GameInvite>()
                .HasIndex(i => i.InviteId)
                .IsUnique();
        }
    }
}