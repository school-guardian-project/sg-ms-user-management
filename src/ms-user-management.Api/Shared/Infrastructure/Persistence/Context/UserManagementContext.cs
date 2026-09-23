using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Configuration;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Context;

public class UserManagementContext : DbContext
{
    public UserManagementContext(DbContextOptions options) : base(options) { }
    
    public DbSet<PersonEntity> Person => Set<PersonEntity>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("UserManagement");

        modelBuilder.Entity<PersonEntity>().ToTable("Person", schema: "UserManagement");

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
    }

    protected UserManagementContext() { }
}