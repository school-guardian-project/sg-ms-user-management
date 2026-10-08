using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Tests.Fakes;
using Xunit;

namespace ms_user_management.Tests.Family;

public class RegisterFamilyServiceTests
{
    private static (Guid ProfileId, RelationType RelationType) Member(
        RelationType relation = RelationType.Student) => (Guid.NewGuid(), relation);

    [Fact]
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

        Assert.Single(repo.Families, f => f.Id == familyId && f.Name == "Perez");
        Assert.Equal(2, members.Count);
        Assert.Equal(2, repo.Members.Count(m => m.FamilyId == familyId));
        Assert.Contains(repo.Members, m => m.ProfileId == parent && m.RelationType == RelationType.Parent);
        Assert.Contains(repo.Members, m => m.ProfileId == child && m.RelationType == RelationType.Student);
    }

    [Fact]
    public async Task ExecuteAsync_ConMiembroYaAsociado_RechazaCon409()
    {
        var repo = new InMemoryFamilyRepository();
        var taken = Guid.NewGuid();
        repo.Members.Add((Guid.NewGuid(), Guid.NewGuid(), taken, RelationType.Student));
        var service = new RegisterFamilyService(repo);

        var ex = await Assert.ThrowsAsync<ProfileAlreadyInFamilyException>(() =>
            service.ExecuteAsync("Perez", null,
                new List<(Guid, RelationType)> { (taken, RelationType.Student) }));

        Assert.Contains("pertenece a otra familia", ex.Message);
        Assert.Contains(taken, ex.ProfileIds);
        Assert.Empty(repo.Families);
    }

    [Fact]
    public async Task ExecuteAsync_ConPerfilesDuplicadosEnLote_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);
        var same = Guid.NewGuid();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync("Perez", null,
                new List<(Guid, RelationType)>
                {
                    (same, RelationType.Parent),
                    (same, RelationType.Student)
                }));

        Assert.Contains("Duplicated", ex.Message);
        Assert.Empty(repo.Families);
    }

    [Fact]
    public async Task ExecuteAsync_ConLoteVacio_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync("Perez", null, new List<(Guid, RelationType)>()));
    }

    [Fact]
    public async Task ExecuteAsync_SinNombre_Rechaza()
    {
        var repo = new InMemoryFamilyRepository();
        var service = new RegisterFamilyService(repo);
        var (profileId, relation) = Member();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync("  ", null,
                new List<(Guid, RelationType)> { (profileId, relation) }));
    }
}
