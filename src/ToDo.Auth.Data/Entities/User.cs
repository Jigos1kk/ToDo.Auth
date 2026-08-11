namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Пользователь платформы (заказчик или фрилансер).
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>
    /// Email, используется для входа. Хранится в нижнем регистре.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля (PBKDF2). Пароль в открытом виде нигде не хранится.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<Role> Roles { get; set; } = [];

    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
