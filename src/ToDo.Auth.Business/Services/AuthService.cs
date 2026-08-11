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
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<OperationResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var userName = request.UserName.Trim();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Пользователь с таким email уже зарегистрирован.");
        }

        if (await _userRepository.ExistsByUserNameAsync(userName, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Пользователь с таким именем уже существует.");
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

        // Токены здесь не выдаются: после регистрации пользователь проходит вход
        return OperationResult<UserDto>.Success(MapToDto(user));
    }

    public async Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            // Единое сообщение, чтобы не раскрывать, существует ли пользователь
            return OperationResult<AuthResponse>.Failure("Неверный email или пароль.");
        }

        // Каждый успешный вход — новая сессия
        var now = DateTime.UtcNow;
        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CreatedAt = now,
            LastActivityAt = now,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
        await _sessionRepository.AddAsync(session, cancellationToken);

        var (response, _) = await CreateAuthResponseAsync(user, session, cancellationToken);
        return OperationResult<AuthResponse>.Success(response);
    }

    public async Task<OperationResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive || !existingToken.Session.IsActive)
        {
            return OperationResult<AuthResponse>.Failure("Refresh-токен недействителен или истёк.");
        }

        var session = existingToken.Session;

        // Ротация: старый токен отзываем, взамен выдаём новый в рамках той же сессии
        var (response, newToken) = await CreateAuthResponseAsync(session.User, session, cancellationToken);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenId = newToken.Id;
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        session.LastActivityAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return OperationResult<AuthResponse>.Success(response);
    }

    private async Task<(AuthResponse Response, RefreshToken RefreshToken)> CreateAuthResponseAsync(User user, UserSession session, CancellationToken cancellationToken)
    {
        var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(user);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            TokenHash = _tokenService.HashRefreshToken(refreshTokenValue),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenLifetimeDays)
        };
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            User = MapToDto(user)
        };
        return (response, refreshToken);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        UserName = user.UserName,
        Roles = user.Roles.Select(role => role.Name).ToList()
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
