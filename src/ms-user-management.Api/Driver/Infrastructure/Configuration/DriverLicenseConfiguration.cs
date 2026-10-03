using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_user_management.Api.Driver.Infrastructure.Persistence;

namespace ms_user_management.Api.Driver.Infrastructure.Configuration;

public class DriverLicenseConfiguration : IEntityTypeConfiguration<DriverLicenseEntity>
{
    public void Configure(EntityTypeBuilder<DriverLicenseEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProfileId).IsRequired();
        builder.Property(x => x.LicenseNumber).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.LicenseNumber).IsUnique();
        builder.Property(x => x.LicenseExpirationDate).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}
