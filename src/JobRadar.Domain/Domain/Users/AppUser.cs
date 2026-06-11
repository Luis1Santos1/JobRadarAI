using JobRadar.Domain.Profiles;
using JobRadar.Domain.Users;
using JobRadar.SharedKernel;
using System.Data;

namespace JobRadar.Domain.Domain.Users;

public sealed class AppUser : BaseEntity
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private AppUser()
    {
    }

    public AppUser(string name, string email, string passwordHash)
    {
        Name = name;
        Email = email.ToLowerInvariant().Trim();
        PasswordHash = passwordHash;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DeveloperProfile? DeveloperProfile { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public void Update(string name, string email)
    {
        Name = name;
        Email = email.ToLowerInvariant().Trim();
        MarkAsUpdated();
    }

    public void AddRole(Role role)
    {
        if (_userRoles.Any(userRole => userRole.RoleId == role.Id))
        {
            return;
        }

        _userRoles.Add(new UserRole(Id, role.Id));
    }

    public void AddRefreshToken(string tokenHash, DateTimeOffset expiresAt, string? createdByIp)
    {
        _refreshTokens.Add(new RefreshToken(Id, tokenHash, expiresAt, createdByIp));
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}