namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Одноразовый токен для подтверждения email после регистрации.
/// В базе хранится только хеш токена.
/// </summary>
public class EmailConfirmationToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// SHA-256 хеш значения токена.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}