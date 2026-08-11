using Microsoft.Extensions.Options;
using ToDo.Auth.Business.Dtos;
using ToDo.Auth.Business.Options;
using ToDo.Auth.Data.Entities;
using ToDo.Auth.Data.Repositories;

namespace ToDo.Auth.Business.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var userName = request.UserName.Trim();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return AuthResult.Failure("Пользователь с таким email уже зарегистрирован.");
        }

        if (await _userRepository.ExistsByUserNameAsync(userName, cancellationToken))
        {
            return AuthResult.Failure("Пользователь с таким именем уже существует.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        var defaultRole = await _roleRepository.GetByNameAsync(RoleNames.User, cancellationToken);
        if (defaultRole is not null)
        {
            user.Roles.Add(defaultRole);
        }

        await _userRepository.AddAsync(user, cancellationToken);

        return AuthResult.Success(await CreateAuthResponseAsync(user, cancellationToken));
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Единое сообщение, чтобы не раскрывать, существует ли пользователь
            return AuthResult.Failure("Неверный email или пароль.");
        }

        return AuthResult.Success(await CreateAuthResponseAsync(user, cancellationToken));
    }

    public async Task<AuthResult> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return AuthResult.Failure("Refresh-токен недействителен или истёк.");
        }

        // Ротация: старый токен отзываем, взамен выдаём новую пару
        var response = await CreateAuthResponseAsync(existingToken.User, cancellationToken);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = _tokenService.HashRefreshToken(response.RefreshToken);
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        return AuthResult.Success(response);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user);

        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays);

        await _refreshTokenRepository.AddAsync(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshToken),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = refreshTokenExpiresAt
        }, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                Roles = user.Roles.Select(role => role.Name).ToList()
            }
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
