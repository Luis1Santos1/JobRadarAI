using JobRadar.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class DeveloperProfileConfiguration : IEntityTypeConfiguration<DeveloperProfile>
{
    public void Configure(EntityTypeBuilder<DeveloperProfile> builder)
    {
        builder.ToTable("DeveloperProfiles");

        builder.HasKey(profile => profile.Id);

        builder.Property(profile => profile.TargetTitle)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(profile => profile.TargetSeniority)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(profile => profile.ProfessionalSummary)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(profile => profile.DesiredWorkModel)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(profile => profile.DesiredContractType)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(profile => profile.DesiredLocations)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(profile => profile.SalaryExpectation)
            .HasPrecision(18, 2);

        builder.Property(profile => profile.PositiveKeywords)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(profile => profile.NegativeKeywords)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasIndex(profile => profile.UserId)
            .IsUnique();

        builder.HasMany(profile => profile.Technologies)
            .WithOne(item => item.DeveloperProfile)
            .HasForeignKey(item => item.DeveloperProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(profile => profile.Technologies)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}