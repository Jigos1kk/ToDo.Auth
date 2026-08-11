using System.ComponentModel.DataAnnotations;
using ToDo.Auth.Business.Validation;

namespace ToDo.Auth.Business.Dtos;

/// <summary>
/// Данные для регистрации пользователя.
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    [StringLength(256, ErrorMessage = "Email не может превышать 256 символов.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Имя пользователя обязательно.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Имя пользователя должно содержать от 3 до 100 символов.")]
    [RegularExpression(@"^[\p{L}0-9_.-]+$", ErrorMessage = "Имя пользователя может содержать только буквы, цифры и символы _, ., -.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен.")]
    [StringLength(100, ErrorMessage = "Пароль не может превышать 100 символов.")]
    [PasswordStrength]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Данные для входа пользователя.
/// </summary>
public class LoginRequest
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен.")]
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Данные для обновления пары токенов.
/// </summary>
public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh-токен обязателен.")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Пара токенов и сведения о пользователе после успешной аутентификации.
/// </summary>
public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAt { get; set; }

    public UserDto User { get; set; } = new();
}

/// <summary>
/// Представление пользователя для клиента.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public bool EmailConfirmed { get; set; }

    public List<string> Roles { get; set; } = [];
}