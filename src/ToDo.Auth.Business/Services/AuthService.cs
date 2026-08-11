using Microsoft.Extensions.Options;
using ToDo.Auth.Business.Dtos;
using ToDo.Auth.Business.Options;
using ToDo.Auth.Data.Entities;
using ToDo.Auth.Data.Repositories;

namespace ToDo.Auth.Business.Services;

public class AuthService : IAuthService
{
    private const int EmailConfirmationTokenLifetimeHours = 24;
    private const int PasswordResetTokenLifetimeHours = 1;

    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailConfirmationTokenRepository _emailConfirmationTokenRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserSessionRepository sessionRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailConfirmationTokenRepository emailConfirmationTokenRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailService emailService,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailConfirmationTokenRepository = emailConfirmationTokenRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _emailService = emailService;
        _jwtOptions = jwtOptions.Value;
    }

    // ── Регистрация ──────────────────────────────────────────────

    public async Task<OperationResult<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var normalizedUserName = NormalizeUserName(request.UserName);

        if (await _userRepository.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Пользователь с таким email уже зарегистрирован.");
        }

        if (await _userRepository.ExistsByNormalizedUserNameAsync(normalizedUserName, cancellationToken))
        {
            return OperationResult<UserDto>.Failure("Пользователь с таким именем уже существует.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.Trim(),
            NormalizedEmail = normalizedEmail,
            UserName = request.UserName.Trim(),
            NormalizedUserName = normalizedUserName,
            PasswordHash = _passwordHasher.Hash(request.Password),
            EmailConfirmed = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Назначаем выбранную роль (Customer или Freelancer) + базовую User
        var roleName = request.Role is "Customer" ? RoleNames.Customer : RoleNames.Freelancer;
        var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (role is not null)
        {
            user.Roles.Add(role);
        }

        var userRole = await _roleRepository.GetByNameAsync(RoleNames.User, cancellationToken);
        if (userRole is not null)
        {
            user.Roles.Add(userRole);
        }

        await _userRepository.AddAsync(user, cancellationToken);

        // Создаём токен подтверждения почты и отправляем письмо
        await CreateAndSendEmailConfirmationAsync(user, cancellationToken);

        return OperationResult<UserDto>.Success(MapToDto(user));
    }

    // ── Подтверждение email ──────────────────────────────────────

    public async Task<OperationResult<UserDto>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashToken(request.Token);
        var token = await _emailConfirmationTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (token is null || token.IsExpired)
        {
            return OperationResult<UserDto>.Failure("Токен подтверждения недействителен или истёк.");
        }

        var user = token.User;
        user.EmailConfirmed = true;
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _emailConfirmationTokenRepository.DeleteAsync(token, cancellationToken);

        return OperationResult<UserDto>.Success(MapToDto(user));
    }

    // ── Вход ─────────────────────────────────────────────────────

    public async Task<OperationResult<AuthResponse>> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return OperationResult<AuthResponse>.Failure("Неверный email или пароль.");
        }

        if (!user.IsActive)
        {
            return OperationResult<AuthResponse>.Failure("Учётная запись деактивирована.");
        }

        if (!user.EmailConfirmed)
        {
            return OperationResult<AuthResponse>.Failure(
                "Email не подтверждён. Проверьте почту и перейдите по ссылке из письма.");
        }

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

    // ── Обновление токенов ───────────────────────────────────────

    public async Task<OperationResult<AuthResponse>> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive || !existingToken.Session.IsActive)
        {
            return OperationResult<AuthResponse>.Failure("Refresh-токен недействителен или истёк.");
        }

        var session = existingToken.Session;

        var (response, newToken) = await CreateAuthResponseAsync(session.User, session, cancellationToken);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenId = newToken.Id;
        await _refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);

        session.LastActivityAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return OperationResult<AuthResponse>.Success(response);
    }

    // ── Профиль пользователя ─────────────────────────────────────

    public async Task<OperationResult<UserDto>> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return OperationResult<UserDto>.Failure("Пользователь не найден.");
        }

        return OperationResult<UserDto>.Success(MapToDto(user));
    }

    // ── Смена пароля ─────────────────────────────────────────────

    public async Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return OperationResult.Failure("Пользователь не найден.");
        }

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return OperationResult.Failure("Текущий пароль указан неверно.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        return OperationResult.Success();
    }

    // ── Восстановление пароля (запрос) ───────────────────────────

    public async Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);
        var user = await _userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        // Всегда возвращаем успех, чтобы не раскрывать, существует ли email
        if (user is null || !user.IsActive)
        {
            return OperationResult.Success();
        }

        var tokenValue = _tokenService.GenerateToken();
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(tokenValue),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(PasswordResetTokenLifetimeHours)
        };
        await _passwordResetTokenRepository.AddAsync(token, cancellationToken);

        await _emailService.SendEmailAsync(
            user.Email,
            "Восстановление пароля ToDo",
            $"Для сброса пароля используйте следующий код (действителен {PasswordResetTokenLifetimeHours} ч.):\n\n{tokenValue}",
            cancellationToken);

        return OperationResult.Success();
    }

    // ── Восстановление пароля (сброс) ────────────────────────────

    public async Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = _tokenService.HashToken(request.Token);
        var token = await _passwordResetTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (token is null || !token.IsValid)
        {
            return OperationResult.Failure("Токен сброса пароля недействителен, истёк или уже использован.");
        }

        var user = token.User;
        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        await _userRepository.UpdateAsync(user, cancellationToken);

        token.IsUsed = true;
        await _passwordResetTokenRepository.UpdateAsync(token, cancellationToken);

        return OperationResult.Success();
    }

    // ── Приватные методы ─────────────────────────────────────────

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

    private async Task CreateAndSendEmailConfirmationAsync(User user, CancellationToken cancellationToken)
    {
        var tokenValue = _tokenService.GenerateToken();
        var token = new EmailConfirmationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = _tokenService.HashToken(tokenValue),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(EmailConfirmationTokenLifetimeHours)
        };
        await _emailConfirmationTokenRepository.AddAsync(token, cancellationToken);

        await _emailService.SendEmailAsync(
            user.Email,
            "Подтверждение регистрации ToDo",
            $"Для подтверждения email используйте следующий код (действителен {EmailConfirmationTokenLifetimeHours} ч.):\n\n{tokenValue}",
            cancellationToken);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        UserName = user.UserName,
        EmailConfirmed = user.EmailConfirmed,
        Roles = user.Roles.Select(role => role.Name).ToList()
    };

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static string NormalizeUserName(string userName) => userName.Trim().ToUpperInvariant();
}