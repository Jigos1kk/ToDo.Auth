using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data.Repositories;

public interface IEmailConfirmationTokenRepository
{
    Task<EmailConfirmationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<EmailConfirmationToken> AddAsync(EmailConfirmationToken token, CancellationToken cancellationToken = default);

    Task DeleteAsync(EmailConfirmationToken token, CancellationToken cancellationToken = default);
}