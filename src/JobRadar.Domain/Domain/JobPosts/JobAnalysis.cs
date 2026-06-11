using JobRadar.SharedKernel;

namespace JobRadar.Domain.JobPosts;

public sealed class JobAnalysis : BaseEntity
{
    private JobAnalysis()
    {
    }

    private JobAnalysis(
        Guid userId,
        Guid jobPostId,
        string promptVersion,
        string model,
        bool isJobPost,
        string detectedTitle,
        string detectedCompany,
        string seniority,
        string workModel,
        string contractType,
        string location,
        string requiredTechnologiesJson,
        string niceToHaveTechnologiesJson,
        string responsibilitiesJson,
        string requirementsJson,
        string benefitsJson,
        string redFlagsJson,
        string fitReasonsJson,
        string concernsJson,
        string summary,
        int aiFitScore,
        int hybridScore,
        decimal confidenceScore,
        string recommendation,
        string rawModelResponse,
        string? errorMessage,
        DateTimeOffset completedAt)
    {
        UserId = userId;
        JobPostId = jobPostId;
        PromptVersion = promptVersion;
        Model = model;
        IsJobPost = isJobPost;
        DetectedTitle = detectedTitle;
        DetectedCompany = detectedCompany;
        Seniority = seniority;
        WorkModel = workModel;
        ContractType = contractType;
        Location = location;
        RequiredTechnologiesJson = requiredTechnologiesJson;
        NiceToHaveTechnologiesJson = niceToHaveTechnologiesJson;
        ResponsibilitiesJson = responsibilitiesJson;
        RequirementsJson = requirementsJson;
        BenefitsJson = benefitsJson;
        RedFlagsJson = redFlagsJson;
        FitReasonsJson = fitReasonsJson;
        ConcernsJson = concernsJson;
        Summary = summary;
        AiFitScore = aiFitScore;
        HybridScore = hybridScore;
        ConfidenceScore = confidenceScore;
        Recommendation = recommendation;
        RawModelResponse = rawModelResponse;
        ErrorMessage = errorMessage;
        CompletedAt = completedAt;
    }

    public Guid UserId { get; private set; }

    public Guid JobPostId { get; private set; }

    public JobPost JobPost { get; private set; } = null!;

    public string PromptVersion { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public bool IsJobPost { get; private set; }

    public string DetectedTitle { get; private set; } = string.Empty;

    public string DetectedCompany { get; private set; } = string.Empty;

    public string Seniority { get; private set; } = string.Empty;

    public string WorkModel { get; private set; } = string.Empty;

    public string ContractType { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public string RequiredTechnologiesJson { get; private set; } = "[]";

    public string NiceToHaveTechnologiesJson { get; private set; } = "[]";

    public string ResponsibilitiesJson { get; private set; } = "[]";

    public string RequirementsJson { get; private set; } = "[]";

    public string BenefitsJson { get; private set; } = "[]";

    public string RedFlagsJson { get; private set; } = "[]";

    public string FitReasonsJson { get; private set; } = "[]";

    public string ConcernsJson { get; private set; } = "[]";

    public string Summary { get; private set; } = string.Empty;

    public int AiFitScore { get; private set; }

    public int HybridScore { get; private set; }

    public decimal ConfidenceScore { get; private set; }

    public string Recommendation { get; private set; } = string.Empty;

    public string RawModelResponse { get; private set; } = string.Empty;

    public string? ErrorMessage { get; private set; }

    public DateTimeOffset CompletedAt { get; private set; }

    public static JobAnalysis CreateCompleted(
        Guid userId,
        Guid jobPostId,
        string promptVersion,
        string model,
        bool isJobPost,
        string detectedTitle,
        string detectedCompany,
        string seniority,
        string workModel,
        string contractType,
        string location,
        string requiredTechnologiesJson,
        string niceToHaveTechnologiesJson,
        string responsibilitiesJson,
        string requirementsJson,
        string benefitsJson,
        string redFlagsJson,
        string fitReasonsJson,
        string concernsJson,
        string summary,
        int aiFitScore,
        int hybridScore,
        decimal confidenceScore,
        string recommendation,
        string rawModelResponse)
    {
        return new JobAnalysis(
            userId,
            jobPostId,
            promptVersion,
            model,
            isJobPost,
            detectedTitle,
            detectedCompany,
            seniority,
            workModel,
            contractType,
            location,
            requiredTechnologiesJson,
            niceToHaveTechnologiesJson,
            responsibilitiesJson,
            requirementsJson,
            benefitsJson,
            redFlagsJson,
            fitReasonsJson,
            concernsJson,
            summary,
            aiFitScore,
            hybridScore,
            confidenceScore,
            recommendation,
            rawModelResponse,
            null,
            DateTimeOffset.UtcNow);
    }

    public static JobAnalysis CreateFailed(
        Guid userId,
        Guid jobPostId,
        string promptVersion,
        string model,
        string rawModelResponse,
        string errorMessage)
    {
        return new JobAnalysis(
            userId,
            jobPostId,
            promptVersion,
            model,
            false,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            "[]",
            string.Empty,
            0,
            0,
            0,
            "AnalysisFailed",
            rawModelResponse,
            errorMessage,
            DateTimeOffset.UtcNow);
    }
}