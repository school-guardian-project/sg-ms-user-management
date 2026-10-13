using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class GetFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    [Test]
    public async Task ExecuteAsync_ConFamilia_RetornaNombreParentHijosYDescripcion()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez", "Ruta 5");
        var parent = Guid.NewGuid();
        var childOne = Guid.NewGuid();
        var childTwo = Guid.NewGuid();

        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, parent, RelationType.Parent));
        repo.Members.Add((Guid.NewGuid(), family.Id, childOne, RelationType.Student));
        repo.Members.Add((Guid.NewGuid(), family.Id, childTwo, RelationType.Student));

        var result = await new GetFamilyService(repo).ExecuteAsync(family.Id);

        Assert.That(result.Id, Is.EqualTo(family.Id));
        Assert.That(result.Name, Is.EqualTo("Perez"));
        Assert.That(result.ParentProfileId, Is.EqualTo(parent));
        Assert.That(result.Children, Is.EqualTo(new[] { childOne, childTwo }));
        Assert.That(result.Description, Is.EqualTo("Ruta 5"));
    }

    [Test]
    public async Task ExecuteAsync_ConFamiliaSinParent_DejaParentEnNulo()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Student));

        var result = await new GetFamilyService(repo).ExecuteAsync(family.Id);

        Assert.That(result.ParentProfileId, Is.Null);
        Assert.That(result.Children, Has.Exactly(1).Items);
    }

    [Test]
    public async Task ExecuteAsync_FamiliaInexistente_LanzaError()
    {
        var repo = new InMemoryFamilyRepository();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => new GetFamilyService(repo).ExecuteAsync(Guid.NewGuid()));

        Assert.That(ex.Message, Does.Contain("not found"));
    }
}
