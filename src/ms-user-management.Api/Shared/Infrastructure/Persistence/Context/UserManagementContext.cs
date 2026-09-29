using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Family.Infrastructure.Configuration;
using ms_user_management.Api.Family.Infrastructure.Persistence;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Configuration;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Context;

public class UserManagementContext : DbContext
{
    public UserManagementContext(DbContextOptions options) : base(options) { }
    
    public DbSet<PersonEntity> Person => Set<PersonEntity>();
    public DbSet<FamilyEntity> Families => Set<FamilyEntity>();
    public DbSet<FamilyMemberEntity> FamilyMembers => Set<FamilyMemberEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("UserManagement");

        modelBuilder.Entity<PersonEntity>().ToTable("Person", schema: "UserManagement");
        modelBuilder.Entity<FamilyEntity>().ToTable("Family", schema: "UserManagement");
        modelBuilder.Entity<FamilyMemberEntity>().ToTable("FamilyMember", schema: "UserManagement");

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new FamilyConfiguration());
        modelBuilder.ApplyConfiguration(new FamilyMemberConfiguration());
    }

    protected UserManagementContext() { }
}