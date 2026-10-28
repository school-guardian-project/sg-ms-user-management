using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Family.Infrastructure.Persistence;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using NUnit.Framework;

namespace ms_user_management.Tests.Family;

[TestFixture]
public sealed class FamilyMemberSchemaMetadataTests
{
    [Test]
    public void FamilyMemberConstraintsMatchTheCheckedInSqlSchema()
    {
        var options = new DbContextOptionsBuilder<UserManagementContext>()
            .UseSqlServer("Server=localhost;Database=metadata-only;User Id=unused;Password=unused;TrustServerCertificate=True")
            .Options;

        using var context = new UserManagementContext(options);
        var createScript = context.Database.GenerateCreateScript();
        var familyMember = context.Model.FindEntityType(typeof(FamilyMemberEntity))!;

        var familyForeignKey = familyMember.GetForeignKeys()
            .Single(foreignKey => foreignKey.Properties.Any(property => property.Name == nameof(FamilyMemberEntity.FamilyId)));

        Assert.Multiple(() =>
        {
            Assert.That(createScript, Does.Contain("[RelationshipType] IN ('Parent', 'Student')"));
            Assert.That(familyForeignKey.DeleteBehavior, Is.EqualTo(DeleteBehavior.NoAction));
        });
    }
}
