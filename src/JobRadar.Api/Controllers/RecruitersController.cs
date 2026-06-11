using JobRadar.Api.Extensions;
using JobRadar.Domain.Recruiters;
using JobRadar.Infrastructure.Normalization;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/recruiters")]
[Authorize]
public sealed class RecruitersController : ControllerBase
{
    private readonly JobRadarDbContext _dbContext;
    private readonly ContactNormalizationService _normalizer;

    public RecruitersController(
        JobRadarDbContext dbContext,
        ContactNormalizationService normalizer)
    {
        _dbContext = dbContext;
        _normalizer = normalizer;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] ConnectionStatus? status,
        [FromQuery] string? tag,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var query = _dbContext.Recruiters
            .Include(recruiter => recruiter.Tags)
            .Where(recruiter => recruiter.UserId == userId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = _normalizer.NormalizeText(search);

            query = query.Where(recruiter =>
                recruiter.NormalizedName.Contains(normalizedSearch) ||
                recruiter.NormalizedCompany.Contains(normalizedSearch));
        }

        if (status.HasValue)
        {
            query = query.Where(recruiter => recruiter.ConnectionStatus == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = _normalizer.NormalizeText(tag);

            query = query.Where(recruiter =>
                recruiter.Tags.Any(item => item.NormalizedName == normalizedTag));
        }

        var recruiters = await query
            .OrderByDescending(recruiter => recruiter.CreatedAt)
            .Select(recruiter => ToResponse(recruiter, false))
            .ToListAsync(cancellationToken);

        return Ok(recruiters);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var recruiter = await _dbContext.Recruiters
            .Include(item => item.Tags)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return NotFound();
        }

        var duplicates = await FindDuplicatesAsync(userId, recruiter.NormalizedName, recruiter.NormalizedCompany, recruiter.NormalizedLinkedInUrl, cancellationToken);

        return Ok(ToResponse(recruiter, duplicates.Any(), duplicates));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRecruiterRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var normalizedName = _normalizer.NormalizeText(request.Name);
        var normalizedCompany = _normalizer.NormalizeText(request.Company);
        var normalizedLinkedInUrl = _normalizer.NormalizeLinkedInUrl(request.LinkedInUrl);

        var recruiterAlreadyExists = await RecruiterAlreadyExistsAsync(
            userId,
            normalizedName,
            normalizedCompany,
            normalizedLinkedInUrl,
            cancellationToken);

        if (recruiterAlreadyExists)
        {
            return Conflict(new
            {
                message = "Recruiter already exists.",
                matchReason = !string.IsNullOrWhiteSpace(normalizedLinkedInUrl)
                    ? "LinkedInUrl"
                    : "NameCompany"
            });
        }

        var duplicates = await FindDuplicatesAsync(
            userId,
            normalizedName,
            normalizedCompany,
            normalizedLinkedInUrl,
            cancellationToken);

        var recruiter = new Recruiter(
            userId,
            request.Name.Trim(),
            normalizedName,
            request.Title.Trim(),
            request.Company.Trim(),
            normalizedCompany,
            request.LinkedInUrl?.Trim(),
            normalizedLinkedInUrl,
            request.Email?.Trim(),
            request.Phone?.Trim(),
            request.Location?.Trim(),
            request.ConnectionStatus,
            request.Source.Trim(),
            request.Notes?.Trim());

        foreach (var tag in request.Tags.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            recruiter.AddTag(tag.Trim(), _normalizer.NormalizeText(tag));
        }

        _dbContext.Recruiters.Add(recruiter);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = recruiter.Id },
            ToResponse(recruiter, duplicates.Any(), duplicates));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRecruiterRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var recruiter = await _dbContext.Recruiters
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return NotFound();
        }

        var normalizedName = _normalizer.NormalizeText(request.Name);
        var normalizedCompany = _normalizer.NormalizeText(request.Company);
        var normalizedLinkedInUrl = _normalizer.NormalizeLinkedInUrl(request.LinkedInUrl);

        recruiter.Update(
            request.Name.Trim(),
            normalizedName,
            request.Title.Trim(),
            request.Company.Trim(),
            normalizedCompany,
            request.LinkedInUrl?.Trim(),
            normalizedLinkedInUrl,
            request.Email?.Trim(),
            request.Phone?.Trim(),
            request.Location?.Trim(),
            request.Source.Trim(),
            request.Notes?.Trim());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Recruiter updated successfully."
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var recruiter = await _dbContext.Recruiters
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return NotFound();
        }

        _dbContext.Recruiters.Remove(recruiter);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Recruiter deleted successfully."
        });
    }

    [HttpPost("{id:guid}/change-connection-status")]
    public async Task<IActionResult> ChangeConnectionStatus(Guid id, ChangeConnectionStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var recruiter = await _dbContext.Recruiters
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return NotFound();
        }

        recruiter.ChangeConnectionStatus(request.Status);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Recruiter connection status changed successfully."
        });
    }

    [HttpPost("{id:guid}/tags")]
    public async Task<IActionResult> AddTag(Guid id, AddRecruiterTagRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var recruiter = await _dbContext.Recruiters
            .Include(item => item.Tags)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return NotFound();
        }

        recruiter.AddTag(request.Name.Trim(), _normalizer.NormalizeText(request.Name));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Tag added successfully."
        });
    }

    [HttpDelete("{id:guid}/tags/{tagId:guid}")]
    public async Task<IActionResult> RemoveTag(Guid id, Guid tagId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var recruiter = await _dbContext.Recruiters
            .Include(item => item.Tags)
            .FirstOrDefaultAsync(item => item.Id == id && item.UserId == userId, cancellationToken);

        if (recruiter is null)
        {
            return NotFound();
        }

        recruiter.RemoveTag(tagId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Tag removed successfully."
        });
    }

    private async Task<List<DuplicateCandidateResponse>> FindDuplicatesAsync(
        Guid userId,
        string normalizedName,
        string normalizedCompany,
        string? normalizedLinkedInUrl,
        CancellationToken cancellationToken)
    {
        var duplicates = new List<DuplicateCandidateResponse>();

        if (!string.IsNullOrWhiteSpace(normalizedLinkedInUrl))
        {
            var knownConnectionByUrl = await _dbContext.KnownConnections
                .Where(item => item.UserId == userId && item.NormalizedLinkedInUrl == normalizedLinkedInUrl)
                .Select(item => new DuplicateCandidateResponse(
                    "KnownConnection",
                    item.Id,
                    item.Name,
                    item.Company,
                    item.LinkedInUrl,
                    "LinkedInUrl"))
                .ToListAsync(cancellationToken);

            duplicates.AddRange(knownConnectionByUrl);
        }

        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            var knownConnectionByName = await _dbContext.KnownConnections
                .Where(item =>
                    item.UserId == userId &&
                    item.NormalizedName == normalizedName &&
                    item.NormalizedCompany == normalizedCompany)
                .Select(item => new DuplicateCandidateResponse(
                    "KnownConnection",
                    item.Id,
                    item.Name,
                    item.Company,
                    item.LinkedInUrl,
                    "NameCompany"))
                .ToListAsync(cancellationToken);

            duplicates.AddRange(knownConnectionByName);
        }

        return duplicates
            .GroupBy(item => new { item.Type, item.Id })
            .Select(group => group.First())
            .ToList();
    }

    private static RecruiterResponse ToResponse(
        Recruiter recruiter,
        bool hasPossibleDuplicate,
        IReadOnlyCollection<DuplicateCandidateResponse>? duplicates = null)
    {
        return new RecruiterResponse(
            recruiter.Id,
            recruiter.Name,
            recruiter.Title,
            recruiter.Company,
            recruiter.LinkedInUrl,
            recruiter.Email,
            recruiter.Phone,
            recruiter.Location,
            recruiter.ConnectionStatus,
            recruiter.Source,
            recruiter.Notes,
            recruiter.LastContactAt,
            recruiter.Tags.Select(tag => new RecruiterTagResponse(tag.Id, tag.Name)).ToArray(),
            hasPossibleDuplicate,
            duplicates ?? []);
    }

    private async Task<bool> RecruiterAlreadyExistsAsync(
    Guid userId,
    string normalizedName,
    string normalizedCompany,
    string? normalizedLinkedInUrl,
    CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(normalizedLinkedInUrl))
        {
            var existsByUrl = await _dbContext.Recruiters
                .AnyAsync(item =>
                    item.UserId == userId &&
                    item.NormalizedLinkedInUrl == normalizedLinkedInUrl,
                    cancellationToken);

            if (existsByUrl)
            {
                return true;
            }
        }

        return await _dbContext.Recruiters
            .AnyAsync(item =>
                item.UserId == userId &&
                item.NormalizedName == normalizedName &&
                item.NormalizedCompany == normalizedCompany,
                cancellationToken);
    }
}

public sealed record CreateRecruiterRequest(
    string Name,
    string Title,
    string Company,
    string? LinkedInUrl,
    string? Email,
    string? Phone,
    string? Location,
    ConnectionStatus ConnectionStatus,
    string Source,
    string? Notes,
    IReadOnlyCollection<string> Tags);

public sealed record UpdateRecruiterRequest(
    string Name,
    string Title,
    string Company,
    string? LinkedInUrl,
    string? Email,
    string? Phone,
    string? Location,
    string Source,
    string? Notes);

public sealed record ChangeConnectionStatusRequest(
    ConnectionStatus Status);

public sealed record AddRecruiterTagRequest(
    string Name);

public sealed record RecruiterResponse(
    Guid Id,
    string Name,
    string Title,
    string Company,
    string? LinkedInUrl,
    string? Email,
    string? Phone,
    string? Location,
    ConnectionStatus ConnectionStatus,
    string Source,
    string? Notes,
    DateTimeOffset? LastContactAt,
    IReadOnlyCollection<RecruiterTagResponse> Tags,
    bool HasPossibleDuplicate,
    IReadOnlyCollection<DuplicateCandidateResponse> PossibleDuplicates);

public sealed record RecruiterTagResponse(
    Guid Id,
    string Name);

public sealed record DuplicateCandidateResponse(
    string Type,
    Guid Id,
    string Name,
    string? Company,
    string? LinkedInUrl,
    string MatchReason);