using System.Text.Json;
using JobRadar.Domain.JobPosts;
using JobRadar.Domain.Profiles;
using JobRadar.Infrastructure.AI.Prompts;
using JobRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobRadar.Infrastructure.AI;

public sealed class LocalJobAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false
    };

    private readonly JobRadarDbContext _dbContext;
    private readonly LocalAiClient _localAiClient;
    private readonly HybridJobScoreCalculator _scoreCalculator;
    private readonly LocalAiSettings _settings;
    private readonly ILogger<LocalJobAnalysisService> _logger;

    public LocalJobAnalysisService(
        JobRadarDbContext dbContext,
        LocalAiClient localAiClient,
        HybridJobScoreCalculator scoreCalculator,
        IOptions<LocalAiSettings> settings,
        ILogger<LocalJobAnalysisService> logger)
    {
        _dbContext = dbContext;
        _localAiClient = localAiClient;
        _scoreCalculator = scoreCalculator;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<JobAnalysisExecutionResult> AnalyzeAsync(
        Guid userId,
        Guid jobPostId,
        CancellationToken cancellationToken)
    {
        var jobPost = await _dbContext.JobPosts
            .Include(item => item.Recruiter)
            .FirstOrDefaultAsync(
                item => item.Id == jobPostId && item.UserId == userId,
                cancellationToken);

        if (jobPost is null)
        {
            return JobAnalysisExecutionResult.NotFound();
        }

        var profile = await _dbContext.DeveloperProfiles
            .Include(item => item.Technologies)
            .ThenInclude(item => item.Technology)
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        jobPost.RequestAnalysis();

        await _dbContext.SaveChangesAsync(cancellationToken);

        var rawResponse = string.Empty;

        try
        {
            var prompt = JobAnalysisPromptCatalog.BuildJobAnalysisPrompt(jobPost, profile);

            rawResponse = await _localAiClient.GenerateAsync(prompt, cancellationToken);

            var aiResponse = JobAnalysisAiResponseParser.Parse(rawResponse);

            var hybridScore = _scoreCalculator.Calculate(profile, aiResponse);

            await RemoveExistingAnalysisAsync(userId, jobPostId, cancellationToken);

            var analysis = JobAnalysis.CreateCompleted(
                userId,
                jobPostId,
                JobAnalysisPromptCatalog.JobAnalysisV1,
                _settings.Model,
                aiResponse.IsJobPost,
                aiResponse.DetectedTitle,
                aiResponse.DetectedCompany,
                aiResponse.Seniority,
                aiResponse.WorkModel,
                aiResponse.ContractType,
                aiResponse.Location,
                Serialize(aiResponse.RequiredTechnologies),
                Serialize(aiResponse.NiceToHaveTechnologies),
                Serialize(aiResponse.Responsibilities),
                Serialize(aiResponse.Requirements),
                Serialize(aiResponse.Benefits),
                Serialize(aiResponse.RedFlags),
                Serialize(aiResponse.FitReasons),
                Serialize(aiResponse.Concerns),
                aiResponse.Summary,
                Math.Clamp(aiResponse.AiFitScore, 0, 100),
                Math.Clamp(hybridScore, 0, 100),
                Math.Clamp(aiResponse.ConfidenceScore, 0, 1),
                aiResponse.Recommendation,
                rawResponse);

            _dbContext.JobAnalyses.Add(analysis);

            jobPost.MarkAnalysisCompleted();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return JobAnalysisExecutionResult.Completed(analysis.Id, analysis.HybridScore);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to analyze job post {JobPostId} for user {UserId}",
                jobPostId,
                userId);

            await RemoveExistingAnalysisAsync(userId, jobPostId, cancellationToken);

            var failedAnalysis = JobAnalysis.CreateFailed(
                userId,
                jobPostId,
                JobAnalysisPromptCatalog.JobAnalysisV1,
                _settings.Model,
                rawResponse,
                exception.Message);

            _dbContext.JobAnalyses.Add(failedAnalysis);

            jobPost.MarkAnalysisFailed(exception.Message);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return JobAnalysisExecutionResult.Failed(failedAnalysis.Id, exception.Message);
        }
    }

    private async Task RemoveExistingAnalysisAsync(
        Guid userId,
        Guid jobPostId,
        CancellationToken cancellationToken)
    {
        var existingAnalysis = await _dbContext.JobAnalyses
            .FirstOrDefaultAsync(
                item => item.UserId == userId && item.JobPostId == jobPostId,
                cancellationToken);

        if (existingAnalysis is null)
        {
            return;
        }

        _dbContext.JobAnalyses.Remove(existingAnalysis);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string Serialize(IReadOnlyCollection<string> values)
    {
        return JsonSerializer.Serialize(values, JsonOptions);
    }
}

public sealed record JobAnalysisExecutionResult(
    bool Found,
    bool Success,
    bool IsFailed,
    Guid? AnalysisId,
    int? HybridScore,
    string? ErrorMessage)
{
    public static JobAnalysisExecutionResult NotFound()
    {
        return new JobAnalysisExecutionResult(false, false, false, null, null, null);
    }

    public static JobAnalysisExecutionResult Completed(Guid analysisId, int hybridScore)
    {
        return new JobAnalysisExecutionResult(true, true, false, analysisId, hybridScore, null);
    }

    public static JobAnalysisExecutionResult Failed(Guid analysisId, string errorMessage)
    {
        return new JobAnalysisExecutionResult(true, false, true, analysisId, null, errorMessage);
    }
}