using JobRadar.Domain.Domain.Users;
using JobRadar.Domain.Users;
using JobRadar.Infrastructure.Auth;
using JobRadar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly JobRadarDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        JobRadarDbContext dbContext,
        PasswordHasher passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();

        var emailAlreadyExists = await _dbContext.Users
            .AnyAsync(user => user.Email == email, cancellationToken);

        if (emailAlreadyExists)
        {
            return Conflict(new
            {
                message = "Email already registered."
            });
        }

        var defaultRole = await _dbContext.Roles
            .FirstAsync(role => role.Name == "User", cancellationToken);

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new AppUser(request.Name.Trim(), email, passwordHash);

        user.AddRole(defaultRole);

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiration();

        _dbContext.Users.Add(user);

        _dbContext.RefreshTokens.Add(new RefreshToken(
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAt,
            GetIpAddress()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user, ["User"]);

        return Ok(new AuthResponse(
            accessToken,
            refreshToken,
            _jwtTokenService.GetAccessTokenExpiration(),
            new AuthUserResponse(user.Id, user.Name, user.Email, ["User"])));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLowerInvariant().Trim();

        var user = await _dbContext.Users
            .Include(item => item.UserRoles)
            .ThenInclude(item => item.Role)
            .FirstOrDefaultAsync(item => item.Email == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials."
            });
        }

        var passwordIsValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordIsValid)
        {
            return Unauthorized(new
            {
                message = "Invalid credentials."
            });
        }

        var roles = user.UserRoles
            .Select(item => item.Role.Name)
            .ToArray();

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiration();

        _dbContext.RefreshTokens.Add(new RefreshToken(
            user.Id,
            refreshTokenHash,
            refreshTokenExpiresAt,
            GetIpAddress()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);

        return Ok(new AuthResponse(
            accessToken,
            refreshToken,
            _jwtTokenService.GetAccessTokenExpiration(),
            new AuthUserResponse(user.Id, user.Name, user.Email, roles)));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashRefreshToken(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(item => item.User)
            .ThenInclude(item => item.UserRoles)
            .ThenInclude(item => item.Role)
            .FirstOrDefaultAsync(item => item.TokenHash == tokenHash, cancellationToken);

        if (storedToken is null || !storedToken.IsActive || !storedToken.User.IsActive)
        {
            return Unauthorized(new
            {
                message = "Invalid refresh token."
            });
        }

        storedToken.Revoke(GetIpAddress());

        var user = storedToken.User;

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtTokenService.HashRefreshToken(newRefreshToken);
        var newRefreshTokenExpiresAt = _jwtTokenService.GetRefreshTokenExpiration();

        _dbContext.RefreshTokens.Add(new RefreshToken(
            user.Id,
            newRefreshTokenHash,
            newRefreshTokenExpiresAt,
            GetIpAddress()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        var roles = user.UserRoles
            .Select(item => item.Role.Name)
            .ToArray();

        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);

        return Ok(new AuthResponse(
            accessToken,
            newRefreshToken,
            _jwtTokenService.GetAccessTokenExpiration(),
            new AuthUserResponse(user.Id, user.Name, user.Email, roles)));
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    AuthUserResponse User);

public sealed record AuthUserResponse(
    Guid Id,
    string Name,
    string Email,
    IReadOnlyCollection<string> Roles);