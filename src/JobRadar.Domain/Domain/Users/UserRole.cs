namespace JobRadar.Domain.Domain.Users;

public sealed class UserRole
{
    private UserRole()
    {
    }

    public UserRole(Guid userId, Guid roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public Guid RoleId { get; private set; }

    public Role Role { get; private set; } = null!;
}