using JobRadar.SharedKernel;

namespace JobRadar.Domain.Recruiters;

public sealed class Recruiter : BaseEntity
{
    private Recruiter()
    {
    }

    public Recruiter(
        Guid userId,
        string name,
        string normalizedName,
        string title,
        string company,
        string normalizedCompany,
        string? linkedInUrl,
        string? normalizedLinkedInUrl,
        string? email,
        string? phone,
        string? location,
        ConnectionStatus connectionStatus,
        string source,
        string? notes)
    {
        UserId = userId;
        Name = name;
        NormalizedName = normalizedName;
        Title = title;
        Company = company;
        NormalizedCompany = normalizedCompany;
        LinkedInUrl = linkedInUrl;
        NormalizedLinkedInUrl = normalizedLinkedInUrl;
        Email = email;
        Phone = phone;
        Location = location;
        ConnectionStatus = connectionStatus;
        Source = source;
        Notes = notes;
    }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Company { get; private set; } = string.Empty;

    public string NormalizedCompany { get; private set; } = string.Empty;

    public string? LinkedInUrl { get; private set; }

    public string? NormalizedLinkedInUrl { get; private set; }

    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    public string? Location { get; private set; }

    public ConnectionStatus ConnectionStatus { get; private set; }

    public string Source { get; private set; } = string.Empty;

    public string? Notes { get; private set; }

    public DateTimeOffset? LastContactAt { get; private set; }

    public List<RecruiterTag> Tags { get; private set; } = [];

    public void Update(
        string name,
        string normalizedName,
        string title,
        string company,
        string normalizedCompany,
        string? linkedInUrl,
        string? normalizedLinkedInUrl,
        string? email,
        string? phone,
        string? location,
        string source,
        string? notes)
    {
        Name = name;
        NormalizedName = normalizedName;
        Title = title;
        Company = company;
        NormalizedCompany = normalizedCompany;
        LinkedInUrl = linkedInUrl;
        NormalizedLinkedInUrl = normalizedLinkedInUrl;
        Email = email;
        Phone = phone;
        Location = location;
        Source = source;
        Notes = notes;

        MarkAsUpdated();
    }

    public void ChangeConnectionStatus(ConnectionStatus status)
    {
        ConnectionStatus = status;

        if (status is ConnectionStatus.InviteSentManually or ConnectionStatus.Connected)
        {
            LastContactAt = DateTimeOffset.UtcNow;
        }

        MarkAsUpdated();
    }

    public void AddTag(string name, string normalizedName)
    {
        if (Tags.Any(tag => tag.NormalizedName == normalizedName))
        {
            return;
        }

        Tags.Add(new RecruiterTag(Id, name, normalizedName));

        MarkAsUpdated();
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = Tags.FirstOrDefault(item => item.Id == tagId);

        if (tag is null)
        {
            return;
        }

        Tags.Remove(tag);

        MarkAsUpdated();
    }
}