using ToDo.Auth.Data.Entities;

namespace ToDo.Auth.Data.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Role> AddAsync(Role role, CancellationToken cancellationToken = default);
}
