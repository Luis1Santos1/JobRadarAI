using JobRadar.SharedKernel;

namespace JobRadar.Domain.Profiles;

public sealed class Technology : BaseEntity
{
    private Technology()
    {
    }

    public Technology(string name, string category)
    {
        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Category = category.Trim();
    }

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Category { get; private set; } = string.Empty;
}