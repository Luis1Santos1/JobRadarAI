using JobRadar.Domain.Domain.Users;
using JobRadar.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobRadar.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var logger = scope.ServiceProvider.GetRequiredService<ILogger<JobRadarDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobRadarDbContext>();

        await SeedRolesAsync(dbContext, logger);
    }

    private static async Task SeedRolesAsync(JobRadarDbContext dbContext, ILogger logger)
    {
        var roles = new[]
        {
            new Role("Admin", "Administrator role"),
            new Role("User", "Default user role")
        };

        foreach (var role in roles)
        {
            var exists = await dbContext.Roles.AnyAsync(item => item.Name == role.Name);

            if (exists)
            {
                continue;
            }

            dbContext.Roles.Add(role);

            logger.LogInformation("Seeded role {RoleName}", role.Name);
        }

        await dbContext.SaveChangesAsync();
    }
}