using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data.Repositories;

public interface IUserSessionRepository
{
    Task<UserSession> AddAsync(UserSession session, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
}
