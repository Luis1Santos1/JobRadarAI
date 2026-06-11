using JobRadar.SharedKernel;

namespace JobRadar.Domain.Recruiters;

public sealed class RecruiterTag : BaseEntity
{
    private RecruiterTag()
    {
    }

    public RecruiterTag(Guid recruiterId, string name, string normalizedName)
    {
        RecruiterId = recruiterId;
        Name = name;
        NormalizedName = normalizedName;
    }

    public Guid RecruiterId { get; private set; }

    public Recruiter Recruiter { get; private set; } = null!;

    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;
}