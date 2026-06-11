using JobRadar.Infrastructure.AI;
using JobRadar.Infrastructure.Auth;
using JobRadar.Infrastructure.Normalization;
using JobRadar.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        services.Configure<LocalAiSettings>(configuration.GetSection("LocalAi"));

        services.AddScoped<HybridJobScoreCalculator>();
        services.AddScoped<LocalJobAnalysisService>();

        services.AddHttpClient<LocalAiClient>((serviceProvider, httpClient) =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<LocalAiSettings>>()
                .Value;

            httpClient.BaseAddress = new Uri(settings.BaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds <= 0 ? 180 : settings.TimeoutSeconds);
        });
        services.AddScoped<PasswordHasher>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<ContactNormalizationService>();

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