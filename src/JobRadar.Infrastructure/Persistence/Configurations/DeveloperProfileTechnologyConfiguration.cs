using JobRadar.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class DeveloperProfileTechnologyConfiguration : IEntityTypeConfiguration<DeveloperProfileTechnology>
{
    public void Configure(EntityTypeBuilder<DeveloperProfileTechnology> builder)
    {
        builder.ToTable("DeveloperProfileTechnologies");

        builder.HasKey(item => new
        {
            item.DeveloperProfileId,
            item.TechnologyId
        });

        builder.Property(item => item.Level)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(item => item.IsPrimary)
            .IsRequired();

        builder.Property(item => item.Weight)
            .IsRequired();

        builder.HasOne(item => item.Technology)
            .WithMany()
            .HasForeignKey(item => item.TechnologyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}