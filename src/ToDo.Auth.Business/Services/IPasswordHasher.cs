namespace ToDo.Auth.Business.Services;

/// <summary>
/// Хеширование и проверка паролей.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
