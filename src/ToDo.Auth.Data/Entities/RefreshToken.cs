namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Refresh-токен пользователя. В базе хранится только хеш токена,
/// само значение выдаётся клиенту один раз при создании.
/// </summary>
public class RefreshToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// SHA-256 хеш значения токена.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Время отзыва токена (при ротации). Null — токен ещё действует.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Хеш токена, которым этот токен был заменён при ротации.
    /// </summary>
    public string? ReplacedByTokenHash { get; set; }

    public User User { get; set; } = null!;

    /// <summary>
    /// Токен действует, если он не отозван и не истёк.
    /// </summary>
    public bool IsActive => RevokedAt is null && DateTime.UtcNow < ExpiresAt;
}
