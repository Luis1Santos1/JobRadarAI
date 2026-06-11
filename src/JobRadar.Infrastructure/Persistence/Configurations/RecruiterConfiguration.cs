using JobRadar.Domain.Recruiters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class RecruiterConfiguration : IEntityTypeConfiguration<Recruiter>
{
    public void Configure(EntityTypeBuilder<Recruiter> builder)
    {
        builder.ToTable("Recruiters");

        builder.HasKey(recruiter => recruiter.Id);

        builder.Property(recruiter => recruiter.Name)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(recruiter => recruiter.NormalizedName)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(recruiter => recruiter.Title)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(recruiter => recruiter.Company)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(recruiter => recruiter.NormalizedCompany)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(recruiter => recruiter.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(recruiter => recruiter.NormalizedLinkedInUrl)
            .HasMaxLength(500);

        builder.Property(recruiter => recruiter.Email)
            .HasMaxLength(256);

        builder.Property(recruiter => recruiter.Phone)
            .HasMaxLength(80);

        builder.Property(recruiter => recruiter.Location)
            .HasMaxLength(180);

        builder.Property(recruiter => recruiter.ConnectionStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(recruiter => recruiter.Source)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(recruiter => recruiter.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(recruiter => new
        {
            recruiter.UserId,
            recruiter.NormalizedLinkedInUrl
        });

        builder.HasIndex(recruiter => new
        {
            recruiter.UserId,
            recruiter.NormalizedName,
            recruiter.NormalizedCompany
        });

        builder.HasIndex(recruiter => new
        {
            recruiter.UserId,
            recruiter.ConnectionStatus
        });

        builder.HasMany(recruiter => recruiter.Tags)
            .WithOne(tag => tag.Recruiter)
            .HasForeignKey(tag => tag.RecruiterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}