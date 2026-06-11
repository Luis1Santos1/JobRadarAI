using JobRadar.Api.Extensions;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly JobRadarDbContext _dbContext;

    public MeController(JobRadarDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var user = await _dbContext.Users
            .Include(item => item.UserRoles)
            .ThenInclude(item => item.Role)
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);

        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(new
        {
            user.Id,
            user.Name,
            user.Email,
            Roles = user.UserRoles.Select(item => item.Role.Name)
        });
    }
}