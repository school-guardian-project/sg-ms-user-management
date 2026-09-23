using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Configuration;

public class PersonConfiguration : IEntityTypeConfiguration<PersonEntity>
{
    public void Configure(EntityTypeBuilder<PersonEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50); 
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(50); 
        builder.Property(x => x.IdentificationType).HasConversion<string>().HasMaxLength(20).IsRequired(); 
        builder.Property(x => x.IdentificationNumber).IsRequired().HasMaxLength(20); 
        builder.Property(x => x.Email).HasMaxLength(50); 
        builder.Property(x => x.Phone); 
        builder.Property(x => x.ResidenceAddress).HasMaxLength(50); 
        builder.Property(x => x.DateBirth).IsRequired(); 
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
    }
}