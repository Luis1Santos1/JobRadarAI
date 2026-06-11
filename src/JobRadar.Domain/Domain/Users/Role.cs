using JobRadar.SharedKernel;

namespace JobRadar.Domain.Domain.Users;

public sealed class Role : BaseEntity
{
    private readonly List<UserRole> _userRoles = [];

    private Role()
    {
    }

    public Role(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;
}