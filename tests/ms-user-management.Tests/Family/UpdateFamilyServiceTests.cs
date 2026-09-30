using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

public class UpdateFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    [Fact]
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
        Assert.Equal("Perez Gomez", updated.Name);
        Assert.Equal("Bloque 3", updated.Observations);
        Assert.Equal(2, repo.Members.Count(m => m.FamilyId == family.Id));
        Assert.DoesNotContain(repo.Members, m => m.ProfileId == oldChild);
        Assert.Contains(repo.Members, m => m.ProfileId == newChild && m.RelationType == RelationType.Student);
    }

    [Fact]
    public async Task ExecuteAsync_FamiliaInexistente_LanzaError()
    {
        var repo = new InMemoryFamilyRepository();
        var profileId = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                Guid.NewGuid(), "Perez", null,
                new List<(Guid, RelationType)> { (profileId, RelationType.Parent) }));

        Assert.Contains("not found", ex.Message);
    }

    [Fact]
    public async Task ExecuteAsync_SinNombre_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        repo.Families.Add(family);

        await Assert.ThrowsAsync<ArgumentException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                family.Id, "   ", null,
                new List<(Guid, RelationType)> { (Guid.NewGuid(), RelationType.Parent) }));
    }

    [Fact]
    public async Task ExecuteAsync_PerfilAsociadoA_OtraFamilia_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        var taken = Guid.NewGuid();

        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Parent));
        repo.Families.Add(NewFamily("Gomez"));
        repo.Members.Add((Guid.NewGuid(), repo.Families[1].Id, taken, RelationType.Student));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                family.Id, "Perez", null,
                new List<(Guid, RelationType)> { (taken, RelationType.Parent) }));

        Assert.Contains("already associated", ex.Message);
        Assert.Equal("Perez", repo.Families.Single(f => f.Id == family.Id).Name);
    }

    [Fact]
    public async Task ExecuteAsync_PerfilDuplicado_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        var same = Guid.NewGuid();

        repo.Families.Add(family);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new UpdateFamilyService(repo).ExecuteAsync(
                family.Id, "Perez", null,
                new List<(Guid, RelationType)>
                {
                    (same, RelationType.Parent),
                    (same, RelationType.Student)
                }));

        Assert.Contains("Duplicated", ex.Message);
    }
}
