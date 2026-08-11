namespace ToDo.Auth.Data.Entities;

/// <summary>
/// Роль пользователя (например, User или Admin).
/// </summary>
public class Role
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<User> Users { get; set; } = [];
}
