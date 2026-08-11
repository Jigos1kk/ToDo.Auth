using System.ComponentModel.DataAnnotations;

namespace ToDo.Auth.Business.Options;

/// <summary>
/// Настройки JWT-аутентификации (секция "Jwt" в конфигурации).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Симметричный ключ подписи токенов. В production задаётся
    /// через переменную окружения Jwt__SecretKey, а не хранится в репозитории.
    /// </summary>
    [Required]
    [MinLength(32, ErrorMessage = "SecretKey должен содержать не менее 32 символов.")]
    public string SecretKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    [Range(1, 365)]
    public int RefreshTokenLifetimeDays { get; set; } = 7;
}
