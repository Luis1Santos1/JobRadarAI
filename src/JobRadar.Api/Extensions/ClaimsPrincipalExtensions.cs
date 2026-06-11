using System.Security.Claims;

namespace JobRadar.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value =
            user.FindFirstValue("userId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user id claim.");
        }

        return userId;
    }
}