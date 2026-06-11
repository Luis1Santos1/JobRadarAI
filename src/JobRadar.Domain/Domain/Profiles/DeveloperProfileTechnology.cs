namespace JobRadar.Domain.Profiles;

public sealed class DeveloperProfileTechnology
{
    private DeveloperProfileTechnology()
    {
    }

    public DeveloperProfileTechnology(
        Guid developerProfileId,
        Guid technologyId,
        string level,
        bool isPrimary,
        int weight)
    {
        DeveloperProfileId = developerProfileId;
        TechnologyId = technologyId;
        Level = level;
        IsPrimary = isPrimary;
        Weight = weight;
    }

    public Guid DeveloperProfileId { get; private set; }

    public DeveloperProfile DeveloperProfile { get; private set; } = null!;

    public Guid TechnologyId { get; private set; }

    public Technology Technology { get; private set; } = null!;

    public string Level { get; private set; } = string.Empty;

    public bool IsPrimary { get; private set; }

    public int Weight { get; private set; }
}