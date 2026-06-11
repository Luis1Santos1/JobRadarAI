using JobRadar.Api.Extensions;
using JobRadar.Domain.JobPosts;
using JobRadar.Infrastructure.AI;
using JobRadar.Infrastructure.Normalization;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/job-posts")]
[Authorize]
public sealed class JobPostsController : ControllerBase
{
    private readonly JobRadarDbContext _dbContext;
    private readonly ContactNormalizationService _normalizer;
    private readonly LocalJobAnalysisService _jobAnalysisService;

    public JobPostsController(
        JobRadarDbContext dbContext,
        ContactNormalizationService normalizer,
        LocalJobAnalysisService jobAnalysisService)
    {
        _dbContext = dbContext;
        _normalizer = normalizer;
        _jobAnalysisService = jobAnalysisService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] JobPostAnalysisStatus? analysisStatus,
        [FromQuery] Guid? recruiterId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var query = _dbContext.JobPosts
            .Include(jobPost => jobPost.Recruiter)
            .Where(jobPost => jobPost.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = _normalizer.NormalizeText(search);

            query = query.Where(jobPost =>
                jobPost.Title.ToUpper().Contains(normalizedSearch) ||
                jobPost.Company.ToUpper().Contains(normalizedSearch) ||
                jobPost.OriginalText.ToUpper().Contains(normalizedSearch));
        }

        if (analysisStatus.HasValue)
        {
            query = query.Where(jobPost => jobPost.AnalysisStatus == analysisStatus.Value);
        }

        if (recruiterId.HasValue)
        {
            query = query.Where(jobPost => jobPost.RecruiterId == recruiterId.Value);
        }

        var posts = await query
            .OrderByDescending(jobPost => jobPost.CreatedAt)
            .Select(jobPost => ToResponse(jobPost, includeOriginalText: false))
            .ToListAsync(cancellationToken);

        return Ok(posts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var jobPost = await _dbContext.JobPosts
            .Include(item => item.Recruiter)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (jobPost is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(jobPost, includeOriginalText: true));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateJobPostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(request.OriginalText))
        {
            return BadRequest(new
            {
                message = "Original text is required."
            });
        }

        if (request.RecruiterId.HasValue)
        {
            var recruiterExists = await _dbContext.Recruiters
                .AnyAsync(
                    recruiter =>
                        recruiter.Id == request.RecruiterId.Value &&
                        recruiter.UserId == userId,
                    cancellationToken);

            if (!recruiterExists)
            {
                return BadRequest(new
                {
                    message = "Recruiter not found for current user."
                });
            }
        }

        var normalizedSourceUrl = _normalizer.NormalizeUrl(request.SourceUrl);

        var possibleDuplicate = await FindPossibleDuplicateAsync(
            userId,
            normalizedSourceUrl,
            request.OriginalText,
            cancellationToken);

        var jobPost = new JobPost(
            userId,
            request.Title.Trim(),
            request.Company.Trim(),
            request.Location?.Trim(),
            request.SourceUrl?.Trim(),
            normalizedSourceUrl,
            request.RecruiterId,
            request.OriginalText.Trim(),
            request.Notes?.Trim());

        _dbContext.JobPosts.Add(jobPost);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = jobPost.Id },
            ToResponse(jobPost, includeOriginalText: true, possibleDuplicate));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateJobPostRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (string.IsNullOrWhiteSpace(request.OriginalText))
        {
            return BadRequest(new
            {
                message = "Original text is required."
            });
        }

        var jobPost = await _dbContext.JobPosts
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (jobPost is null)
        {
            return NotFound();
        }

        if (request.RecruiterId.HasValue)
        {
            var recruiterExists = await _dbContext.Recruiters
                .AnyAsync(
                    recruiter =>
                        recruiter.Id == request.RecruiterId.Value &&
                        recruiter.UserId == userId,
                    cancellationToken);

            if (!recruiterExists)
            {
                return BadRequest(new
                {
                    message = "Recruiter not found for current user."
                });
            }
        }

        jobPost.Update(
            request.Title.Trim(),
            request.Company.Trim(),
            request.Location?.Trim(),
            request.SourceUrl?.Trim(),
            _normalizer.NormalizeUrl(request.SourceUrl),
            request.RecruiterId,
            request.OriginalText.Trim(),
            request.Notes?.Trim());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Job post updated successfully."
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var jobPost = await _dbContext.JobPosts
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (jobPost is null)
        {
            return NotFound();
        }

        _dbContext.JobPosts.Remove(jobPost);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Job post deleted successfully."
        });
    }

    [HttpPost("{id:guid}/request-analysis")]
    public async Task<IActionResult> RequestAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var result = await _jobAnalysisService.AnalyzeAsync(userId, id, cancellationToken);

        if (!result.Found)
        {
            return NotFound();
        }

        if (result.IsFailed)
        {
            return Ok(new
            {
                message = "Job post analysis failed, but the error was saved.",
                analysisStatus = JobPostAnalysisStatus.Failed,
                analysisId = result.AnalysisId,
                error = result.ErrorMessage
            });
        }

        return Ok(new
        {
            message = "Job post analysis completed successfully.",
            analysisStatus = JobPostAnalysisStatus.Completed,
            analysisId = result.AnalysisId,
            hybridScore = result.HybridScore
        });
    }

    [HttpPost("{id:guid}/link-recruiter")]
    public async Task<IActionResult> LinkRecruiter(Guid id, LinkRecruiterRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var jobPost = await _dbContext.JobPosts
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (jobPost is null)
        {
            return NotFound();
        }

        var recruiterExists = await _dbContext.Recruiters
            .AnyAsync(
                recruiter =>
                    recruiter.Id == request.RecruiterId &&
                    recruiter.UserId == userId,
                cancellationToken);

        if (!recruiterExists)
        {
            return BadRequest(new
            {
                message = "Recruiter not found for current user."
            });
        }

        jobPost.LinkRecruiter(request.RecruiterId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Recruiter linked successfully."
        });
    }

    [HttpDelete("{id:guid}/link-recruiter")]
    public async Task<IActionResult> ClearRecruiter(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var jobPost = await _dbContext.JobPosts
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (jobPost is null)
        {
            return NotFound();
        }

        jobPost.ClearRecruiter();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Recruiter unlinked successfully."
        });
    }

    private async Task<JobPostDuplicateResponse?> FindPossibleDuplicateAsync(
        Guid userId,
        string? normalizedSourceUrl,
        string originalText,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(normalizedSourceUrl))
        {
            var duplicateByUrl = await _dbContext.JobPosts
                .Where(item =>
                    item.UserId == userId &&
                    item.NormalizedSourceUrl == normalizedSourceUrl)
                .Select(item => new JobPostDuplicateResponse(
                    item.Id,
                    item.Title,
                    item.Company,
                    item.SourceUrl,
                    "SourceUrl"))
                .FirstOrDefaultAsync(cancellationToken);

            if (duplicateByUrl is not null)
            {
                return duplicateByUrl;
            }
        }

        var textPreview = originalText.Trim();

        if (textPreview.Length > 300)
        {
            textPreview = textPreview[..300];
        }

        return await _dbContext.JobPosts
            .Where(item =>
                item.UserId == userId &&
                item.OriginalText.StartsWith(textPreview))
            .Select(item => new JobPostDuplicateResponse(
                item.Id,
                item.Title,
                item.Company,
                item.SourceUrl,
                "OriginalTextPreview"))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static JobPostResponse ToResponse(
        JobPost jobPost,
        bool includeOriginalText,
        JobPostDuplicateResponse? possibleDuplicate = null)
    {
        var preview = jobPost.OriginalText;

        if (preview.Length > 220)
        {
            preview = $"{preview[..220]}...";
        }

        return new JobPostResponse(
            jobPost.Id,
            jobPost.Title,
            jobPost.Company,
            jobPost.Location,
            jobPost.SourceUrl,
            jobPost.RecruiterId,
            jobPost.Recruiter is null
                ? null
                : new JobPostRecruiterResponse(
                    jobPost.Recruiter.Id,
                    jobPost.Recruiter.Name,
                    jobPost.Recruiter.Company),
            includeOriginalText ? jobPost.OriginalText : null,
            preview,
            jobPost.Notes,
            jobPost.AnalysisStatus,
            jobPost.AnalysisRequestedAt,
            jobPost.AnalysisStartedAt,
            jobPost.AnalysisCompletedAt,
            jobPost.AnalysisError,
            possibleDuplicate is not null,
            possibleDuplicate,
            jobPost.CreatedAt,
            jobPost.UpdatedAt);
    }

    [HttpGet("{id:guid}/analysis")]
    public async Task<IActionResult> GetAnalysis(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var analysis = await _dbContext.JobAnalyses
            .FirstOrDefaultAsync(
                item => item.JobPostId == id && item.UserId == userId,
                cancellationToken);

        if (analysis is null)
        {
            return NotFound(new
            {
                message = "Job analysis not found."
            });
        }

        return Ok(ToAnalysisResponse(analysis));
    }

    private static JobAnalysisResponse ToAnalysisResponse(JobAnalysis analysis)
    {
        return new JobAnalysisResponse(
            analysis.Id,
            analysis.JobPostId,
            analysis.PromptVersion,
            analysis.Model,
            analysis.IsJobPost,
            analysis.DetectedTitle,
            analysis.DetectedCompany,
            analysis.Seniority,
            analysis.WorkModel,
            analysis.ContractType,
            analysis.Location,
            DeserializeList(analysis.RequiredTechnologiesJson),
            DeserializeList(analysis.NiceToHaveTechnologiesJson),
            DeserializeList(analysis.ResponsibilitiesJson),
            DeserializeList(analysis.RequirementsJson),
            DeserializeList(analysis.BenefitsJson),
            DeserializeList(analysis.RedFlagsJson),
            DeserializeList(analysis.FitReasonsJson),
            DeserializeList(analysis.ConcernsJson),
            analysis.Summary,
            analysis.AiFitScore,
            analysis.HybridScore,
            analysis.ConfidenceScore,
            analysis.Recommendation,
            analysis.ErrorMessage,
            analysis.CompletedAt,
            analysis.CreatedAt);
    }

    private static IReadOnlyCollection<string> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<IReadOnlyCollection<string>>(json) ?? [];
    }
}

public sealed record CreateJobPostRequest(
    string Title,
    string Company,
    string? Location,
    string? SourceUrl,
    Guid? RecruiterId,
    string OriginalText,
    string? Notes);

public sealed record UpdateJobPostRequest(
    string Title,
    string Company,
    string? Location,
    string? SourceUrl,
    Guid? RecruiterId,
    string OriginalText,
    string? Notes);

public sealed record LinkRecruiterRequest(
    Guid RecruiterId);

public sealed record JobPostResponse(
    Guid Id,
    string Title,
    string Company,
    string? Location,
    string? SourceUrl,
    Guid? RecruiterId,
    JobPostRecruiterResponse? Recruiter,
    string? OriginalText,
    string OriginalTextPreview,
    string? Notes,
    JobPostAnalysisStatus AnalysisStatus,
    DateTimeOffset? AnalysisRequestedAt,
    DateTimeOffset? AnalysisStartedAt,
    DateTimeOffset? AnalysisCompletedAt,
    string? AnalysisError,
    bool HasPossibleDuplicate,
    JobPostDuplicateResponse? PossibleDuplicate,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record JobPostRecruiterResponse(
    Guid Id,
    string Name,
    string Company);

public sealed record JobPostDuplicateResponse(
    Guid Id,
    string Title,
    string Company,
    string? SourceUrl,
    string MatchReason);

public sealed record JobAnalysisResponse(
    Guid Id,
    Guid JobPostId,
    string PromptVersion,
    string Model,
    bool IsJobPost,
    string DetectedTitle,
    string DetectedCompany,
    string Seniority,
    string WorkModel,
    string ContractType,
    string Location,
    IReadOnlyCollection<string> RequiredTechnologies,
    IReadOnlyCollection<string> NiceToHaveTechnologies,
    IReadOnlyCollection<string> Responsibilities,
    IReadOnlyCollection<string> Requirements,
    IReadOnlyCollection<string> Benefits,
    IReadOnlyCollection<string> RedFlags,
    IReadOnlyCollection<string> FitReasons,
    IReadOnlyCollection<string> Concerns,
    string Summary,
    int AiFitScore,
    int HybridScore,
    decimal ConfidenceScore,
    string Recommendation,
    string? ErrorMessage,
    DateTimeOffset CompletedAt,
    DateTimeOffset CreatedAt);