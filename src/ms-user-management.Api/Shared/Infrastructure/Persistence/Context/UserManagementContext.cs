using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Driver.Infrastructure.Configuration;
using ms_user_management.Api.Driver.Infrastructure.Persistence;
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
    public DbSet<DriverLicenseEntity> DriverLicenses => Set<DriverLicenseEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("UserManagement");

        modelBuilder.Entity<PersonEntity>().ToTable("Person", schema: "UserManagement");
        modelBuilder.Entity<FamilyEntity>().ToTable("Family", schema: "UserManagement");
        modelBuilder.Entity<FamilyMemberEntity>().ToTable("FamilyMember", schema: "UserManagement");
        modelBuilder.Entity<DriverLicenseEntity>().ToTable("DriverLicense", schema: "UserManagement");

        // Solo lectura: Iam.Profile está en el mismo SQL Server y resuelve PersonId -> ProfileId
        // (la licencia se guarda por ProfileId). ponytail: si los esquemas se separan en servicios,
        // mover este puente a ms-iam.
        modelBuilder.Entity<ProfileRefEntity>()
            .HasNoKey()
            .ToTable("Profile", schema: "Iam");

        // Solo lectura: School.SchoolCampus expande el schoolId del JWT del admin
        // a sus sedes para el filtrado multi-tenant de los listados.
        modelBuilder.Entity<SchoolCampusRefEntity>()
            .HasNoKey()
            .ToTable("SchoolCampus", schema: "School");

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new FamilyConfiguration());
        modelBuilder.ApplyConfiguration(new FamilyMemberConfiguration());
        modelBuilder.ApplyConfiguration(new DriverLicenseConfiguration());
    }

    protected UserManagementContext() { }
}