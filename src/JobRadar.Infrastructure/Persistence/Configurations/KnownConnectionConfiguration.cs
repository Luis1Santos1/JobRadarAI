using JobRadar.Domain.Recruiters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class KnownConnectionConfiguration : IEntityTypeConfiguration<KnownConnection>
{
    public void Configure(EntityTypeBuilder<KnownConnection> builder)
    {
        builder.ToTable("KnownConnections");

        builder.HasKey(connection => connection.Id);

        builder.Property(connection => connection.Name)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(connection => connection.NormalizedName)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(connection => connection.LinkedInUrl)
            .HasMaxLength(500);

        builder.Property(connection => connection.NormalizedLinkedInUrl)
            .HasMaxLength(500);

        builder.Property(connection => connection.Company)
            .HasMaxLength(180);

        builder.Property(connection => connection.NormalizedCompany)
            .HasMaxLength(180);

        builder.Property(connection => connection.Title)
            .HasMaxLength(180);

        builder.Property(connection => connection.Email)
            .HasMaxLength(256);

        builder.Property(connection => connection.Location)
            .HasMaxLength(180);

        builder.Property(connection => connection.ConnectionStatus)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(connection => connection.ImportedFrom)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(connection => new
        {
            connection.UserId,
            connection.NormalizedLinkedInUrl
        });

        builder.HasIndex(connection => new
        {
            connection.UserId,
            connection.NormalizedName,
            connection.NormalizedCompany
        });

        builder.HasIndex(connection => new
        {
            connection.UserId,
            connection.ConnectionStatus
        });
    }
}