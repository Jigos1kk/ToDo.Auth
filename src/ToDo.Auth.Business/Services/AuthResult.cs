using ToDo.Auth.Business.Dtos;

namespace ToDo.Auth.Business.Services;

/// <summary>
/// Результат операции аутентификации: либо пара токенов, либо текст ошибки.
/// </summary>
public record AuthResult
{
    public AuthResponse? Response { get; private init; }

    public string? Error { get; private init; }

    public bool IsSuccess => Response is not null;

    public static AuthResult Success(AuthResponse response) => new() { Response = response };

    public static AuthResult Failure(string error) => new() { Error = error };
}
