using Microsoft.AspNetCore.Mvc;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/system")]
public sealed class SystemController : ControllerBase
{
    private readonly ILogger<SystemController> _logger;

    public SystemController(ILogger<SystemController> logger)
    {
        _logger = logger;
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        _logger.LogInformation("System info requested.");

        return Ok(new
        {
            Application = "JobRadar AI",
            Version = "0.1.0",
            Status = "Running",
            Timestamp = DateTimeOffset.UtcNow
        });
    }
}