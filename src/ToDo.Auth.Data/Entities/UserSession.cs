namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Сессия — запись об успешном входе пользователя.
/// К одной сессии привязана цепочка refresh-токенов.
/// </summary>
public class UserSession
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Время последнего обращения (вход или обновление токенов).
    /// </summary>
    public DateTime LastActivityAt { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    /// <summary>
    /// Время завершения сессии (выход). Null — сессия активна.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    public User User { get; set; } = null!;

    public List<RefreshToken> RefreshTokens { get; set; } = [];

    /// <summary>
    /// Сессия действует, если она не завершена.
    /// </summary>
    public bool IsActive => RevokedAt is null;
}
