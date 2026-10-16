using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class ListFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    [Test]
    public async Task ExecuteAsync_ConFamilias_RetornaSoloNombreYParent()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez", "Ruta 5");
        var withoutParent = NewFamily("Gomez");
        var parent = Guid.NewGuid();

        repo.Families.Add(family);
        repo.Families.Add(withoutParent);
        repo.Members.Add((Guid.NewGuid(), family.Id, parent, RelationType.Parent));
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Student));

        var result = (await new ListFamilyService(repo, TenantFakes.NoFilter(new InMemoryPersonProfileReader()), new InMemoryPersonProfileReader()).ExecuteAsync()).ToList();

        Assert.That(result.Count, Is.EqualTo(2));

        var perez = result.Single(f => f.Name == "Perez");
        Assert.That(perez.Id, Is.EqualTo(family.Id));
        Assert.That(perez.ParentProfileId, Is.EqualTo(parent));

        var gomez = result.Single(f => f.Name == "Gomez");
        Assert.That(gomez.Id, Is.EqualTo(withoutParent.Id));
        Assert.That(gomez.ParentProfileId, Is.Null);
    }

    [Test]
    public async Task ExecuteAsync_SinFamilias_RetornaVacio()
    {
        var repo = new InMemoryFamilyRepository();

        var result = await new ListFamilyService(repo, TenantFakes.NoFilter(new InMemoryPersonProfileReader()), new InMemoryPersonProfileReader()).ExecuteAsync();

        Assert.That(result, Is.Empty);
    }
}
