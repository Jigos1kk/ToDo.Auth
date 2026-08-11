using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Business.Services;

/// <summary>
/// Создание access- и refresh-токенов, а также служебных токенов
/// (подтверждение почты, сброс пароля).
/// </summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    string GenerateRefreshToken();

    /// <summary>
    /// Хеш refresh-токена для хранения в базе данных.
    /// </summary>
    string HashRefreshToken(string refreshToken);

    /// <summary>
    /// Генерация криптографически стойкого случайного токена.
    /// Используется для подтверждения email и сброса пароля.
    /// </summary>
    string GenerateToken();

    /// <summary>
    /// SHA-256 хеш токена для хранения в базе данных.
    /// </summary>
    string HashToken(string token);
}