using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ms_user_management.Api.Family.Infrastructure.Persistence;

namespace ms_user_management.Api.Family.Infrastructure.Configuration;

public class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMemberEntity>
{
    public void Configure(EntityTypeBuilder<FamilyMemberEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RelationType).HasColumnName("RelationshipType").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("CK_FamilyMember_RelationType", "[RelationType] IN ('Parent', 'Student')"));
        builder.HasOne(x => x.Family).WithMany().HasForeignKey(x => x.FamilyId).OnDelete(DeleteBehavior.Cascade);
    }
}
