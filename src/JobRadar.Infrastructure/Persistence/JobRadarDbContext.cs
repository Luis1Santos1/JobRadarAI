using JobRadar.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace JobRadar.Infrastructure.Persistence;

public sealed class JobRadarDbContext : DbContext
{
    public JobRadarDbContext(DbContextOptions<JobRadarDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobRadarDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}