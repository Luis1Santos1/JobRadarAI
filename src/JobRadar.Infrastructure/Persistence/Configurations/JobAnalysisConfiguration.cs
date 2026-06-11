using JobRadar.Domain.JobPosts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class JobAnalysisConfiguration : IEntityTypeConfiguration<JobAnalysis>
{
    public void Configure(EntityTypeBuilder<JobAnalysis> builder)
    {
        builder.ToTable("JobAnalyses");

        builder.HasKey(analysis => analysis.Id);

        builder.Property(analysis => analysis.PromptVersion)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(analysis => analysis.Model)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(analysis => analysis.DetectedTitle)
            .HasMaxLength(220)
            .IsRequired();

        builder.Property(analysis => analysis.DetectedCompany)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(analysis => analysis.Seniority)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(analysis => analysis.WorkModel)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(analysis => analysis.ContractType)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(analysis => analysis.Location)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(analysis => analysis.RequiredTechnologiesJson)
            .IsRequired();

        builder.Property(analysis => analysis.NiceToHaveTechnologiesJson)
            .IsRequired();

        builder.Property(analysis => analysis.ResponsibilitiesJson)
            .IsRequired();

        builder.Property(analysis => analysis.RequirementsJson)
            .IsRequired();

        builder.Property(analysis => analysis.BenefitsJson)
            .IsRequired();

        builder.Property(analysis => analysis.RedFlagsJson)
            .IsRequired();

        builder.Property(analysis => analysis.FitReasonsJson)
            .IsRequired();

        builder.Property(analysis => analysis.ConcernsJson)
            .IsRequired();

        builder.Property(analysis => analysis.Summary)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(analysis => analysis.Recommendation)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(analysis => analysis.ConfidenceScore)
            .HasPrecision(5, 2);

        builder.Property(analysis => analysis.RawModelResponse)
            .IsRequired();

        builder.Property(analysis => analysis.ErrorMessage)
            .HasMaxLength(2000);

        builder.HasOne(analysis => analysis.JobPost)
            .WithOne()
            .HasForeignKey<JobAnalysis>(analysis => analysis.JobPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(analysis => new
        {
            analysis.UserId,
            analysis.JobPostId
        }).IsUnique();

        builder.HasIndex(analysis => new
        {
            analysis.UserId,
            analysis.HybridScore
        });
    }
}