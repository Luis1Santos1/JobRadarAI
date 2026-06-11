using JobRadar.Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
    public void Configure(EntityTypeBuilder<Technology> builder)
    {
        builder.ToTable("Technologies");

        builder.HasKey(technology => technology.Id);

        builder.Property(technology => technology.Name)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(technology => technology.NormalizedName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(technology => technology.Category)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(technology => technology.NormalizedName)
            .IsUnique();
    }
}