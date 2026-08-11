using Microsoft.EntityFrameworkCore;
using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserSession> UserSessions => Set<UserSession>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);

            entity.HasMany(e => e.Roles)
                .WithMany(e => e.Users)
                .UsingEntity("UserRoles");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.SessionId);

            entity.HasOne(e => e.Session)
                .WithMany(s => s.RefreshTokens)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
