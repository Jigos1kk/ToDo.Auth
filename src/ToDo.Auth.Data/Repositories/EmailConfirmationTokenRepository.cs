using Microsoft.EntityFrameworkCore;
using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data.Repositories;

public class EmailConfirmationTokenRepository : IEmailConfirmationTokenRepository
{
    private readonly AuthDbContext _context;

    public EmailConfirmationTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<EmailConfirmationToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return await _context.EmailConfirmationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
    }

    public async Task<EmailConfirmationToken> AddAsync(EmailConfirmationToken token, CancellationToken cancellationToken = default)
    {
        _context.EmailConfirmationTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task DeleteAsync(EmailConfirmationToken token, CancellationToken cancellationToken = default)
    {
        _context.EmailConfirmationTokens.Remove(token);
        await _context.SaveChangesAsync(cancellationToken);
    }
}