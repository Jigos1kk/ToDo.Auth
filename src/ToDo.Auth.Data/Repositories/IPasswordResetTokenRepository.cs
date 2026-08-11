using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data.Repositories;

public interface IPasswordResetTokenRepository
{
    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<PasswordResetToken> AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

    Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
}