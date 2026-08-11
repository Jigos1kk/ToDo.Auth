using System.ComponentModel.DataAnnotations;

namespace ToDo.Auth.Business.Options;

/// <summary>
/// Учётные данные администратора, создаваемого при первом запуске (секция "Admin").
/// В production пароль задаётся через переменную окружения Admin__Password.
/// </summary>
public class AdminOptions
{
    public const string SectionName = "Admin";

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
