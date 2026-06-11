using JobRadar.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.TokenHash)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.CreatedByIp)
            .HasMaxLength(80);

        builder.Property(refreshToken => refreshToken.RevokedByIp)
            .HasMaxLength(80);

        builder.HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        builder.HasIndex(refreshToken => new
        {
            refreshToken.UserId,
            refreshToken.ExpiresAt
        });
    }
}