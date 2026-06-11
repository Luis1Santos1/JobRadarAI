using JobRadar.Infrastructure.IA;
using System.Text.Json;

namespace JobRadar.Infrastructure.AI;

public static class JobAnalysisAiResponseParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static JobAnalysisAiResponse Parse(string rawResponse)
    {
        var json = ExtractJsonObject(rawResponse);

        var response = JsonSerializer.Deserialize<JobAnalysisAiResponse>(json, JsonOptions);

        if (response is null)
        {
            throw new InvalidOperationException("Local AI returned an empty JSON response.");
        }

        Validate(response);

        return response;
    }

    private static void Validate(JobAnalysisAiResponse response)
    {
        var errors = new List<string>();

        if (response.IsJobPost && string.IsNullOrWhiteSpace(response.DetectedTitle))
        {
            errors.Add("detectedTitle is required when isJobPost is true.");
        }

        if (response.AiFitScore is < 0 or > 100)
        {
            errors.Add("aiFitScore must be between 0 and 100.");
        }

        if (response.ConfidenceScore is < 0 or > 1)
        {
            errors.Add("confidenceScore must be between 0 and 1.");
        }

        if (string.IsNullOrWhiteSpace(response.Recommendation))
        {
            errors.Add("recommendation is required.");
        }

        if (errors.Count > 0)
        {
            throw new InvalidOperationException($"Invalid Local AI JSON: {string.Join(" | ", errors)}");
        }
    }

    private static string ExtractJsonObject(string rawResponse)
    {
        if (string.IsNullOrWhiteSpace(rawResponse))
        {
            throw new InvalidOperationException("Local AI returned an empty response.");
        }

        var trimmed = rawResponse.Trim();

        var firstBrace = trimmed.IndexOf('{');
        var lastBrace = trimmed.LastIndexOf('}');

        if (firstBrace < 0 || lastBrace < 0 || lastBrace <= firstBrace)
        {
            throw new InvalidOperationException("Local AI response does not contain a valid JSON object.");
        }

        return trimmed[firstBrace..(lastBrace + 1)];
    }
}