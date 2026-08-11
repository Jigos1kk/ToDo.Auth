using System.ComponentModel.DataAnnotations;
using ToDo.Auth.Business.Validation;

namespace ToDo.Auth.Business.Dtos;

/// <summary>
/// Данные для смены пароля авторизованным пользователем.
/// </summary>
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Текущий пароль обязателен.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новый пароль обязателен.")]
    [StringLength(100, ErrorMessage = "Пароль не может превышать 100 символов.")]
    [PasswordStrength]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Данные для запроса восстановления пароля.
/// </summary>
public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "Email обязателен.")]
    [EmailAddress(ErrorMessage = "Некорректный формат email.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Данные для сброса пароля по одноразовому токену.
/// </summary>
public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Токен обязателен.")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Новый пароль обязателен.")]
    [StringLength(100, ErrorMessage = "Пароль не может превышать 100 символов.")]
    [PasswordStrength]
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Данные для подтверждения email.
/// </summary>
public class ConfirmEmailRequest
{
    [Required(ErrorMessage = "Токен подтверждения обязателен.")]
    public string Token { get; set; } = string.Empty;
}