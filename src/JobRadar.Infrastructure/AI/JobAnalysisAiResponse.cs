using System.Text.Json.Serialization;

namespace JobRadar.Infrastructure.IA;

public sealed class JobAnalysisAiResponse
{
    [JsonPropertyName("isJobPost")]
    public bool IsJobPost { get; init; }

    [JsonPropertyName("detectedTitle")]
    public string DetectedTitle { get; init; } = string.Empty;

    [JsonPropertyName("detectedCompany")]
    public string DetectedCompany { get; init; } = string.Empty;

    [JsonPropertyName("seniority")]
    public string Seniority { get; init; } = string.Empty;

    [JsonPropertyName("workModel")]
    public string WorkModel { get; init; } = string.Empty;

    [JsonPropertyName("contractType")]
    public string ContractType { get; init; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; init; } = string.Empty;

    [JsonPropertyName("requiredTechnologies")]
    public List<string> RequiredTechnologies { get; init; } = [];

    [JsonPropertyName("niceToHaveTechnologies")]
    public List<string> NiceToHaveTechnologies { get; init; } = [];

    [JsonPropertyName("responsibilities")]
    public List<string> Responsibilities { get; init; } = [];

    [JsonPropertyName("requirements")]
    public List<string> Requirements { get; init; } = [];

    [JsonPropertyName("benefits")]
    public List<string> Benefits { get; init; } = [];

    [JsonPropertyName("redFlags")]
    public List<string> RedFlags { get; init; } = [];

    [JsonPropertyName("fitReasons")]
    public List<string> FitReasons { get; init; } = [];

    [JsonPropertyName("concerns")]
    public List<string> Concerns { get; init; } = [];

    [JsonPropertyName("summary")]
    public string Summary { get; init; } = string.Empty;

    [JsonPropertyName("aiFitScore")]
    public int AiFitScore { get; init; }

    [JsonPropertyName("confidenceScore")]
    public decimal ConfidenceScore { get; init; }

    [JsonPropertyName("recommendation")]
    public string Recommendation { get; init; } = string.Empty;
}