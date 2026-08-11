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

            // Базовые роли создаются вместе с базой данных
            entity.HasData(
                new Role { Id = new Guid("3d9f2b1a-7c4e-4a8f-9e5b-1c6d8a2f4e70"), Name = RoleNames.User },
                new Role { Id = new Guid("8a1e5c3d-2f6b-4d9a-b7e1-5c3f9a0d2e84"), Name = RoleNames.Admin });
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TokenHash).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
