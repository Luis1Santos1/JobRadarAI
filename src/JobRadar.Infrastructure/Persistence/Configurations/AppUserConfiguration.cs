using JobRadar.Domain.Domain.Users;
using JobRadar.Domain.Profiles;
using JobRadar.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Name)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.IsActive)
            .HasDefaultValue(true);

        builder.Property(user => user.CreatedAt)
            .IsRequired();

        builder.Property(user => user.UpdatedAt);

        builder.HasMany(user => user.UserRoles)
            .WithOne(userRole => userRole.User)
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(refreshToken => refreshToken.User)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(user => user.DeveloperProfile)
            .WithOne(profile => profile.User)
            .HasForeignKey<DeveloperProfile>(profile => profile.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(user => user.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}