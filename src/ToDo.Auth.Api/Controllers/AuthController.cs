using Microsoft.AspNetCore.Mvc;
using ToDo.Auth.Business.Dtos;
using ToDo.Auth.Business.Services;

namespace ToDo.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Регистрация нового пользователя. Токены не выдаются:
    /// после регистрации нужно выполнить вход.
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : Conflict(new { error = result.Error });
    }

    /// <summary>
    /// Вход по email и паролю. Каждый успешный вход создаёт новую сессию
    /// и возвращает пару токенов.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _authService.LoginAsync(request, ipAddress, userAgent, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }

    /// <summary>
    /// Обновление пары токенов по refresh-токену (с ротацией refresh-токена).
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshAsync(request, cancellationToken);
        return result.IsSuccess
            ? Ok(result.Value)
            : Unauthorized(new { error = result.Error });
    }
}
