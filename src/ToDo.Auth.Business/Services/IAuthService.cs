using ToDo.Auth.Business.Dtos;

namespace ToDo.Auth.Business.Services;

public interface IAuthService
{
    Task<OperationResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<UserDto>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    Task<OperationResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult<UserDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);

    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}