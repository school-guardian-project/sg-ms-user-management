using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

public class GetFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    [Fact]
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

        Assert.Equal(family.Id, result.Id);
        Assert.Equal("Perez", result.Name);
        Assert.Equal(parent, result.ParentProfileId);
        Assert.Equal(new[] { childOne, childTwo }, result.Children);
        Assert.Equal("Ruta 5", result.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ConFamiliaSinParent_DejaParentEnNulo()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Student));

        var result = await new GetFamilyService(repo).ExecuteAsync(family.Id);

        Assert.Null(result.ParentProfileId);
        Assert.Single(result.Children);
    }

    [Fact]
    public async Task ExecuteAsync_FamiliaInexistente_LanzaError()
    {
        var repo = new InMemoryFamilyRepository();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new GetFamilyService(repo).ExecuteAsync(Guid.NewGuid()));

        Assert.Contains("not found", ex.Message);
    }
}
