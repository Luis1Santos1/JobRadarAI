using JobRadar.Infrastructure.Auth;
using JobRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobRadar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddScoped<PasswordHasher>();
        services.AddScoped<JwtTokenService>();

        services.AddDbContext<JobRadarDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlServer =>
            {
                sqlServer.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            });
        });

        return services;
    }
}