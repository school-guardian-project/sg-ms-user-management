using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UpdateFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    [Test]
    public async Task ExecuteAsync_ConDatosValidos_ActualizaFamiliaYMiembros()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        var parent = Guid.NewGuid();
        var oldChild = Guid.NewGuid();
        var newChild = Guid.NewGuid();

        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, parent, RelationType.Parent));
        repo.Members.Add((Guid.NewGuid(), family.Id, oldChild, RelationType.Student));

        await new UpdateFamilyService(repo).ExecuteAsync(
            family.Id, "Perez Gomez", "Bloque 3",
            new List<(Guid, RelationType)>
            {
                (parent, RelationType.Parent),
                (newChild, RelationType.Student)
            });

        var updated = repo.Families.Single(f => f.Id == family.Id);
        Assert.That(updated.Name, Is.EqualTo("Perez Gomez"));
        Assert.That(updated.Observations, Is.EqualTo("Bloque 3"));
        Assert.That(repo.Members.Count(m => m.FamilyId == family.Id), Is.EqualTo(2));
        Assert.That(repo.Members.Any(m => m.ProfileId == oldChild), Is.False);
        Assert.That(repo.Members.Any(m => m.ProfileId == newChild && m.RelationType == RelationType.Student), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_FamiliaInexistente_LanzaError()
    {
        var repo = new InMemoryFamilyRepository();
        var profileId = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                Guid.NewGuid(), "Perez", null,
                new List<(Guid, RelationType)> { (profileId, RelationType.Parent) }));

        Assert.That(ex.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task ExecuteAsync_SinNombre_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        repo.Families.Add(family);

        Assert.ThrowsAsync<ArgumentException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                family.Id, "   ", null,
                new List<(Guid, RelationType)> { (Guid.NewGuid(), RelationType.Parent) }));
    }

    [Test]
    public async Task ExecuteAsync_PerfilAsociadoA_OtraFamilia_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        var taken = Guid.NewGuid();

        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Parent));
        repo.Families.Add(NewFamily("Gomez"));
        repo.Members.Add((Guid.NewGuid(), repo.Families[1].Id, taken, RelationType.Student));

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                family.Id, "Perez", null,
                new List<(Guid, RelationType)> { (taken, RelationType.Parent) }));

        Assert.That(ex.Message, Does.Contain("already associated"));
        Assert.That(repo.Families.Single(f => f.Id == family.Id).Name, Is.EqualTo("Perez"));
    }

    [Test]
    public async Task ExecuteAsync_PerfilDuplicado_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        var same = Guid.NewGuid();

        repo.Families.Add(family);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                family.Id, "Perez", null,
                new List<(Guid, RelationType)>
                {
                    (same, RelationType.Parent),
                    (same, RelationType.Student)
                }));

        Assert.That(ex.Message, Does.Contain("Duplicated"));
    }
}
