using JobRadar.Domain.Profiles;
using JobRadar.Infrastructure.IA;
using JobRadar.Infrastructure.Normalization;

namespace JobRadar.Infrastructure.AI;

public sealed class HybridJobScoreCalculator
{
    private readonly ContactNormalizationService _normalizer;

    public HybridJobScoreCalculator(ContactNormalizationService normalizer)
    {
        _normalizer = normalizer;
    }

    public int Calculate(DeveloperProfile? profile, JobAnalysisAiResponse aiResponse)
    {
        if (!aiResponse.IsJobPost)
        {
            return 0;
        }

        var aiScore = Math.Clamp(aiResponse.AiFitScore, 0, 100);
        var technologyScore = CalculateTechnologyScore(profile, aiResponse);
        var seniorityScore = CalculateSeniorityScore(profile, aiResponse);
        var workModelScore = CalculateWorkModelScore(profile, aiResponse);

        var hybridScore =
            (aiScore * 0.50m) +
            (technologyScore * 0.35m) +
            (seniorityScore * 0.10m) +
            (workModelScore * 0.05m);

        return (int)Math.Round(hybridScore, MidpointRounding.AwayFromZero);
    }

    private int CalculateTechnologyScore(DeveloperProfile? profile, JobAnalysisAiResponse aiResponse)
    {
        if (profile is null || profile.Technologies.Count == 0)
        {
            return 50;
        }

        var profileTechs = profile.Technologies
            .Select(item => _normalizer.NormalizeText(item.Technology.Name))
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var requiredTechs = aiResponse.RequiredTechnologies
            .Concat(aiResponse.NiceToHaveTechnologies)
            .Select(_normalizer.NormalizeText)
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requiredTechs.Count == 0)
        {
            return 50;
        }

        var matched = requiredTechs.Count(required =>
            profileTechs.Any(profileTech =>
                profileTech.Contains(required, StringComparison.OrdinalIgnoreCase) ||
                required.Contains(profileTech, StringComparison.OrdinalIgnoreCase)));

        return Math.Clamp((int)Math.Round((decimal)matched / requiredTechs.Count * 100), 0, 100);
    }

    private int CalculateSeniorityScore(DeveloperProfile? profile, JobAnalysisAiResponse aiResponse)
    {
        if (profile is null || string.IsNullOrWhiteSpace(profile.TargetSeniority))
        {
            return 50;
        }

        if (string.IsNullOrWhiteSpace(aiResponse.Seniority) ||
            aiResponse.Seniority.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return 50;
        }

        var target = _normalizer.NormalizeText(profile.TargetSeniority);
        var detected = _normalizer.NormalizeText(aiResponse.Seniority);

        return target.Contains(detected, StringComparison.OrdinalIgnoreCase) ||
               detected.Contains(target, StringComparison.OrdinalIgnoreCase)
            ? 100
            : 50;
    }

    private int CalculateWorkModelScore(DeveloperProfile? profile, JobAnalysisAiResponse aiResponse)
    {
        if (profile is null || string.IsNullOrWhiteSpace(profile.DesiredWorkModel))
        {
            return 50;
        }

        if (string.IsNullOrWhiteSpace(aiResponse.WorkModel) ||
            aiResponse.WorkModel.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            return 50;
        }

        var desired = _normalizer.NormalizeText(profile.DesiredWorkModel);
        var detected = _normalizer.NormalizeText(aiResponse.WorkModel);

        if (desired.Contains("REMOTO", StringComparison.OrdinalIgnoreCase) &&
            detected.Contains("REMOTE", StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        if (desired.Contains("HIBRIDO", StringComparison.OrdinalIgnoreCase) &&
            detected.Contains("HYBRID", StringComparison.OrdinalIgnoreCase))
        {
            return 100;
        }

        return desired.Contains(detected, StringComparison.OrdinalIgnoreCase)
            ? 100
            : 40;
    }
}