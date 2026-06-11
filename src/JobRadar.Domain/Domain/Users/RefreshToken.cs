using JobRadar.Domain.Domain.Users;
using JobRadar.SharedKernel;

namespace JobRadar.Domain.Users;

public sealed class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAt, string? createdByIp)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedByIp = createdByIp;
    }

    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? CreatedByIp { get; private set; }

    public string? RevokedByIp { get; private set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public void Revoke(string? revokedByIp)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        RevokedByIp = revokedByIp;
        MarkAsUpdated();
    }
}