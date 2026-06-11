using JobRadar.Domain.Domain.Users;
using JobRadar.Domain.JobPosts;
using JobRadar.Domain.Profiles;
using JobRadar.Domain.Recruiters;
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

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<DeveloperProfile> DeveloperProfiles => Set<DeveloperProfile>();

    public DbSet<Technology> Technologies => Set<Technology>();

    public DbSet<DeveloperProfileTechnology> DeveloperProfileTechnologies => Set<DeveloperProfileTechnology>();

    public DbSet<Recruiter> Recruiters => Set<Recruiter>();

    public DbSet<RecruiterTag> RecruiterTags => Set<RecruiterTag>();

    public DbSet<KnownConnection> KnownConnections => Set<KnownConnection>();

    public DbSet<JobPost> JobPosts => Set<JobPost>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(JobRadarDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}