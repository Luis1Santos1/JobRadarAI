using JobRadar.Domain.Domain.Users;
using JobRadar.Domain.Users;
using JobRadar.SharedKernel;

namespace JobRadar.Domain.Profiles;

public sealed class DeveloperProfile : BaseEntity
{
    private readonly List<DeveloperProfileTechnology> _technologies = [];

    private DeveloperProfile()
    {
    }

    public DeveloperProfile(Guid userId)
    {
        UserId = userId;
    }

    public Guid UserId { get; private set; }

    public AppUser User { get; private set; } = null!;

    public string TargetTitle { get; private set; } = string.Empty;

    public string TargetSeniority { get; private set; } = string.Empty;

    public string ProfessionalSummary { get; private set; } = string.Empty;

    public string DesiredWorkModel { get; private set; } = string.Empty;

    public string DesiredContractType { get; private set; } = string.Empty;

    public string DesiredLocations { get; private set; } = string.Empty;

    public decimal? SalaryExpectation { get; private set; }

    public string PositiveKeywords { get; private set; } = string.Empty;

    public string NegativeKeywords { get; private set; } = string.Empty;

    public IReadOnlyCollection<DeveloperProfileTechnology> Technologies => _technologies;

    public void Update(
        string targetTitle,
        string targetSeniority,
        string professionalSummary,
        string desiredWorkModel,
        string desiredContractType,
        string desiredLocations,
        decimal? salaryExpectation,
        string positiveKeywords,
        string negativeKeywords)
    {
        TargetTitle = targetTitle;
        TargetSeniority = targetSeniority;
        ProfessionalSummary = professionalSummary;
        DesiredWorkModel = desiredWorkModel;
        DesiredContractType = desiredContractType;
        DesiredLocations = desiredLocations;
        SalaryExpectation = salaryExpectation;
        PositiveKeywords = positiveKeywords;
        NegativeKeywords = negativeKeywords;

        MarkAsUpdated();
    }

    public void AddTechnology(Technology technology, string level, bool isPrimary, int weight)
    {
        if (_technologies.Any(item => item.TechnologyId == technology.Id))
        {
            return;
        }

        _technologies.Add(new DeveloperProfileTechnology(Id, technology.Id, level, isPrimary, weight));
        MarkAsUpdated();
    }

    public void RemoveTechnology(Guid technologyId)
    {
        var item = _technologies.FirstOrDefault(tech => tech.TechnologyId == technologyId);

        if (item is null)
        {
            return;
        }

        _technologies.Remove(item);
        MarkAsUpdated();
    }
}