namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Одноразовый токен для сброса пароля.
/// В базе хранится только хеш токена.
/// </summary>
public class PasswordResetToken
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
    /// Был ли токен уже использован.
    /// </summary>
    public bool IsUsed { get; set; }

    public User User { get; set; } = null!;

    public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
}