using JobRadar.Api.Extensions;
using JobRadar.Domain.Profiles;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/developer-profile")]
[Authorize]
public sealed class DeveloperProfileController : ControllerBase
{
    private readonly JobRadarDbContext _dbContext;

    public DeveloperProfileController(JobRadarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var profile = await _dbContext.DeveloperProfiles
            .Include(item => item.Technologies)
            .ThenInclude(item => item.Technology)
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Developer profile not found."
            });
        }

        return Ok(ToResponse(profile));
    }

    [HttpPut]
    public async Task<IActionResult> Upsert(UpsertDeveloperProfileRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var profile = await _dbContext.DeveloperProfiles
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (profile is null)
        {
            profile = new DeveloperProfile(userId);

            _dbContext.DeveloperProfiles.Add(profile);
        }

        profile.Update(
            request.TargetTitle,
            request.TargetSeniority,
            request.ProfessionalSummary,
            request.DesiredWorkModel,
            request.DesiredContractType,
            request.DesiredLocations,
            request.SalaryExpectation,
            request.PositiveKeywords,
            request.NegativeKeywords);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Developer profile saved successfully."
        });
    }

    [HttpPost("technologies")]
    public async Task<IActionResult> AddTechnology(AddProfileTechnologyRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var profile = await _dbContext.DeveloperProfiles
            .Include(item => item.Technologies)
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Create the developer profile before adding technologies."
            });
        }

        var normalizedName = request.Name.Trim().ToUpperInvariant();

        var technology = await _dbContext.Technologies
            .FirstOrDefaultAsync(item => item.NormalizedName == normalizedName, cancellationToken);

        if (technology is null)
        {
            technology = new Technology(request.Name, request.Category);

            _dbContext.Technologies.Add(technology);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        profile.AddTechnology(
            technology,
            request.Level,
            request.IsPrimary,
            request.Weight);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Technology added successfully."
        });
    }

    [HttpDelete("technologies/{technologyId:guid}")]
    public async Task<IActionResult> RemoveTechnology(Guid technologyId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var profile = await _dbContext.DeveloperProfiles
            .Include(item => item.Technologies)
            .FirstOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (profile is null)
        {
            return NotFound(new
            {
                message = "Developer profile not found."
            });
        }

        profile.RemoveTechnology(technologyId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            message = "Technology removed successfully."
        });
    }

    private static DeveloperProfileResponse ToResponse(DeveloperProfile profile)
    {
        return new DeveloperProfileResponse(
            profile.Id,
            profile.TargetTitle,
            profile.TargetSeniority,
            profile.ProfessionalSummary,
            profile.DesiredWorkModel,
            profile.DesiredContractType,
            profile.DesiredLocations,
            profile.SalaryExpectation,
            profile.PositiveKeywords,
            profile.NegativeKeywords,
            profile.Technologies.Select(item => new ProfileTechnologyResponse(
                item.TechnologyId,
                item.Technology.Name,
                item.Technology.Category,
                item.Level,
                item.IsPrimary,
                item.Weight)).ToArray());
    }
}

public sealed record UpsertDeveloperProfileRequest(
    string TargetTitle,
    string TargetSeniority,
    string ProfessionalSummary,
    string DesiredWorkModel,
    string DesiredContractType,
    string DesiredLocations,
    decimal? SalaryExpectation,
    string PositiveKeywords,
    string NegativeKeywords);

public sealed record AddProfileTechnologyRequest(
    string Name,
    string Category,
    string Level,
    bool IsPrimary,
    int Weight);

public sealed record DeveloperProfileResponse(
    Guid Id,
    string TargetTitle,
    string TargetSeniority,
    string ProfessionalSummary,
    string DesiredWorkModel,
    string DesiredContractType,
    string DesiredLocations,
    decimal? SalaryExpectation,
    string PositiveKeywords,
    string NegativeKeywords,
    IReadOnlyCollection<ProfileTechnologyResponse> Technologies);

public sealed record ProfileTechnologyResponse(
    Guid TechnologyId,
    string Name,
    string Category,
    string Level,
    bool IsPrimary,
    int Weight);