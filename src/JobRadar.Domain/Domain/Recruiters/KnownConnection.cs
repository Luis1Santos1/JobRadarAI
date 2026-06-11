using JobRadar.SharedKernel;

namespace JobRadar.Domain.Recruiters;

public sealed class KnownConnection : BaseEntity
{
    private KnownConnection()
    {
    }

    public KnownConnection(
        Guid userId,
        string name,
        string normalizedName,
        string? linkedInUrl,
        string? normalizedLinkedInUrl,
        string? company,
        string? normalizedCompany,
        string? title,
        string? email,
        string? location,
        ConnectionStatus connectionStatus,
        string importedFrom)
    {
        UserId = userId;
        Name = name;
        NormalizedName = normalizedName;
        LinkedInUrl = linkedInUrl;
        NormalizedLinkedInUrl = normalizedLinkedInUrl;
        Company = company;
        NormalizedCompany = normalizedCompany;
        Title = title;
        Email = email;
        Location = location;
        ConnectionStatus = connectionStatus;
        ImportedFrom = importedFrom;
        ImportedAt = DateTimeOffset.UtcNow;
        LastConfirmedAt = connectionStatus == ConnectionStatus.Connected ? DateTimeOffset.UtcNow : null;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string? LinkedInUrl { get; private set; }

    public string? NormalizedLinkedInUrl { get; private set; }

    public string? Company { get; private set; }

    public string? NormalizedCompany { get; private set; }

    public string? Title { get; private set; }

    public string? Email { get; private set; }

    public string? Location { get; private set; }

    public ConnectionStatus ConnectionStatus { get; private set; }

    public string ImportedFrom { get; private set; } = string.Empty;

    public DateTimeOffset ImportedAt { get; private set; }

    public DateTimeOffset? LastConfirmedAt { get; private set; }

    public void Update(
        string name,
        string normalizedName,
        string? linkedInUrl,
        string? normalizedLinkedInUrl,
        string? company,
        string? normalizedCompany,
        string? title,
        string? email,
        string? location)
    {
        Name = name;
        NormalizedName = normalizedName;
        LinkedInUrl = linkedInUrl;
        NormalizedLinkedInUrl = normalizedLinkedInUrl;
        Company = company;
        NormalizedCompany = normalizedCompany;
        Title = title;
        Email = email;
        Location = location;

        MarkAsUpdated();
    }

    public void ChangeConnectionStatus(ConnectionStatus status)
    {
        ConnectionStatus = status;

        if (status == ConnectionStatus.Connected)
        {
            LastConfirmedAt = DateTimeOffset.UtcNow;
        }

        MarkAsUpdated();
    }
}