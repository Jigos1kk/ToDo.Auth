namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Пользователь платформы (заказчик или фрилансер).
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>
    /// Email, используется для входа. Хранится в исходном регистре.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Нормализованный email (нижний регистр) для быстрого поиска.
    /// </summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Нормализованное имя пользователя (верхний регистр) для быстрого поиска.
    /// </summary>
    public string NormalizedUserName { get; set; } = string.Empty;

    /// <summary>
    /// Хеш пароля (PBKDF2). Пароль в открытом виде нигде не хранится.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Подтверждён ли email пользователя.
    /// </summary>
    public bool EmailConfirmed { get; set; }

    /// <summary>
    /// Активен ли пользователь. Неактивные пользователи не могут войти.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public List<Role> Roles { get; set; } = [];

    public List<UserSession> Sessions { get; set; } = [];
}