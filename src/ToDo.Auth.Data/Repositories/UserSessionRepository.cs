using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly AuthDbContext _context;

    public UserSessionRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<UserSession> AddAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);
        return session;
    }

    public async Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default)
    {
        _context.UserSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
