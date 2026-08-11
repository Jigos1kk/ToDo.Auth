using System.ComponentModel.DataAnnotations;

namespace ToDo.Auth.Business.Validation;

/// <summary>
/// Проверка надёжности пароля: не менее 8 символов, строчная и заглавная
/// буквы, цифра и специальный символ.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class PasswordStrengthAttribute : ValidationAttribute
{
    public const int MinLength = 8;

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        // Пустое значение проверяется атрибутом [Required]
        if (value is not string password)
        {
            return ValidationResult.Success;
        }

        var requirements = new List<string>();
        if (password.Length < MinLength)
        {
            requirements.Add($"не менее {MinLength} символов");
        }

        if (!password.Any(char.IsLower))
        {
            requirements.Add("строчную букву");
        }

        if (!password.Any(char.IsUpper))
        {
            requirements.Add("заглавную букву");
        }

        if (!password.Any(char.IsDigit))
        {
            requirements.Add("цифру");
        }

        if (password.All(char.IsLetterOrDigit))
        {
            requirements.Add("специальный символ");
        }

        return requirements.Count == 0
            ? ValidationResult.Success
            : new ValidationResult($"Пароль должен содержать: {string.Join(", ", requirements)}.");
    }
}
