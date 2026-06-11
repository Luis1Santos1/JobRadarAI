using JobRadar.Domain.JobPosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class JobPostConfiguration : IEntityTypeConfiguration<JobPost>
{
    public void Configure(EntityTypeBuilder<JobPost> builder)
    {
        builder.ToTable("JobPosts");

        builder.HasKey(jobPost => jobPost.Id);

        builder.Property(jobPost => jobPost.Title)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(jobPost => jobPost.Company)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(jobPost => jobPost.Location)
            .HasMaxLength(180);

        builder.Property(jobPost => jobPost.SourceUrl)
            .HasMaxLength(1000);

        builder.Property(jobPost => jobPost.NormalizedSourceUrl)
            .HasMaxLength(1000);

        builder.Property(jobPost => jobPost.OriginalText)
            .IsRequired();

        builder.Property(jobPost => jobPost.Notes)
            .HasMaxLength(2000);

        builder.Property(jobPost => jobPost.AnalysisStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(jobPost => jobPost.AnalysisError)
            .HasMaxLength(1000);

        builder.HasOne(jobPost => jobPost.Recruiter)
            .WithMany()
            .HasForeignKey(jobPost => jobPost.RecruiterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(jobPost => new
        {
            jobPost.UserId,
            jobPost.AnalysisStatus
        });

        builder.HasIndex(jobPost => new
        {
            jobPost.UserId,
            jobPost.RecruiterId
        });

        builder.HasIndex(jobPost => new
        {
            jobPost.UserId,
            jobPost.NormalizedSourceUrl
        });

        builder.HasIndex(jobPost => new
        {
            jobPost.UserId,
            jobPost.CreatedAt
        });
    }
}