using ToDo.Auth.Business.Dtos;

namespace ToDo.Auth.Business.Services;

public interface IAuthService
{
    Task<OperationResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    Task<OperationResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}
