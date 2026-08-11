using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ToDo.Auth.Business.Options;
using ToDo.Auth.Data.Entities;
using ToDo.Auth.Data.Repositories;

namespace ToDo.Auth.Business.Services;

/// <summary>
/// Создаёт при запуске приложения базовые роли (User, Admin)
/// и администратора по умолчанию, если их ещё нет в базе.
/// </summary>
public class AuthDbSeeder
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminOptions _adminOptions;
    private readonly ILogger<AuthDbSeeder> _logger;

    public AuthDbSeeder(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IOptions<AdminOptions> adminOptions,
        ILogger<AuthDbSeeder> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _adminOptions = adminOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleExistsAsync(RoleNames.User, cancellationToken);
        await EnsureRoleExistsAsync(RoleNames.Admin, cancellationToken);
        await EnsureAdminExistsAsync(cancellationToken);
    }

    private async Task EnsureRoleExistsAsync(string roleName, CancellationToken cancellationToken)
    {
        if (await _roleRepository.GetByNameAsync(roleName, cancellationToken) is not null)
        {
            return;
        }

        await _roleRepository.AddAsync(new Role { Id = Guid.NewGuid(), Name = roleName }, cancellationToken);
        _logger.LogInformation("Создана роль {RoleName}.", roleName);
    }

    private async Task EnsureAdminExistsAsync(CancellationToken cancellationToken)
    {
        var email = _adminOptions.Email.Trim().ToLowerInvariant();
        var userName = _adminOptions.UserName.Trim();

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken)
            || await _userRepository.ExistsByUserNameAsync(userName, cancellationToken))
        {
            return;
        }

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            UserName = userName,
            PasswordHash = _passwordHasher.Hash(_adminOptions.Password),
            CreatedAt = DateTime.UtcNow
        };

        var adminRole = await _roleRepository.GetByNameAsync(RoleNames.Admin, cancellationToken);
        if (adminRole is not null)
        {
            admin.Roles.Add(adminRole);
        }

        await _userRepository.AddAsync(admin, cancellationToken);
        _logger.LogInformation("Создан администратор по умолчанию ({Email}).", email);
    }
}
