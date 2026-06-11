using JobRadar.Domain.Recruiters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class RecruiterTagConfiguration : IEntityTypeConfiguration<RecruiterTag>
{
    public void Configure(EntityTypeBuilder<RecruiterTag> builder)
    {
        builder.ToTable("RecruiterTags");

        builder.HasKey(tag => tag.Id);

        builder.Property(tag => tag.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(tag => tag.NormalizedName)
            .HasMaxLength(80)
            .IsRequired();

        builder.HasIndex(tag => new
        {
            tag.RecruiterId,
            tag.NormalizedName
        }).IsUnique();
    }
}