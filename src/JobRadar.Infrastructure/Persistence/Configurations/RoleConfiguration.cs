using JobRadar.Domain.Domain.Users;
using JobRadar.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobRadar.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(role => role.Id);

        builder.Property(role => role.Name)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(role => role.Description)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(role => role.Name)
            .IsUnique();

        builder.Navigation(role => role.UserRoles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}