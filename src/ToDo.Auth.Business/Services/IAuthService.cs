using ToDo.Auth.Business.Dtos;

namespace ToDo.Auth.Business.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
