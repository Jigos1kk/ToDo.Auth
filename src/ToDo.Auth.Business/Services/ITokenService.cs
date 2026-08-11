using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Business.Services;

/// <summary>
/// Создание access- и refresh-токенов.
/// </summary>
public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);

    string GenerateRefreshToken();

    /// <summary>
    /// Хеш refresh-токена для хранения в базе данных.
    /// </summary>
    string HashRefreshToken(string refreshToken);
}
