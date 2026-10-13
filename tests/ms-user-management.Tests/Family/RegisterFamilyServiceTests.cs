using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Family;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class RegisterFamilyServiceTests
{
    private static (Guid ProfileId, RelationType RelationType) Member(
        RelationType relation = RelationType.Student) => (Guid.NewGuid(), relation);

    [Test]
    public async Task ExecuteAsync_ConFamiliaValida_GuardaFamiliaYVinculos()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);
        var parent = Guid.NewGuid();
        var child = Guid.NewGuid();

        var (familyId, members) = await service.ExecuteAsync(
            "Perez", "Ruta 5",
            new List<(Guid, RelationType)>
            {
                (parent, RelationType.Parent),
                (child, RelationType.Student)
            });

        Assert.That(repo.Families.Where(f => f.Id == familyId && f.Name == "Perez"), Has.Exactly(1).Items);
        Assert.That(members.Count, Is.EqualTo(2));
        Assert.That(repo.Members.Count(m => m.FamilyId == familyId), Is.EqualTo(2));
        Assert.That(repo.Members.Any(m => m.ProfileId == parent && m.RelationType == RelationType.Parent), Is.True);
        Assert.That(repo.Members.Any(m => m.ProfileId == child && m.RelationType == RelationType.Student), Is.True);
    }

    [Test]
    public async Task ExecuteAsync_ConMiembroYaAsociado_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var taken = Guid.NewGuid();
        repo.Members.Add((Guid.NewGuid(), Guid.NewGuid(), taken, RelationType.Student));
        var service = new RegisterFamilyService(repo);

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync("Perez", null,
                new List<(Guid, RelationType)> { (taken, RelationType.Student) }));

        Assert.That(ex.Message, Does.Contain("already associated"));
        Assert.That(repo.Families, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_ConPerfilesDuplicadosEnLote_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);
        var same = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync("Perez", null,
                new List<(Guid, RelationType)>
                {
                    (same, RelationType.Parent),
                    (same, RelationType.Student)
                }));

        Assert.That(ex.Message, Does.Contain("Duplicated"));
        Assert.That(repo.Families, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_ConLoteVacio_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync("Perez", null, new List<(Guid, RelationType)>()));
    }

    [Test]
    public async Task ExecuteAsync_SinNombre_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);
        var (profileId, relation) = Member();

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync("  ", null,
                new List<(Guid, RelationType)> { (profileId, relation) }));
    }
}
