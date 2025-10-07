using Microsoft.EntityFrameworkCore;

namespace Server.Models
{
    public class ChessDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<GameInvite> GameInvites { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Log> Logs { get; set; }

        public ChessDbContext(DbContextOptions<ChessDbContext> options) : base(options)
        {
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