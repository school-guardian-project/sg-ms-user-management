using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

public class ListFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    [Fact]
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

        Assert.Equal(2, result.Count);

        var perez = result.Single(f => f.Name == "Perez");
        Assert.Equal(family.Id, perez.Id);
        Assert.Equal(parent, perez.ParentProfileId);

        var gomez = result.Single(f => f.Name == "Gomez");
        Assert.Equal(withoutParent.Id, gomez.Id);
        Assert.Null(gomez.ParentProfileId);
    }

    [Fact]
    public async Task ExecuteAsync_SinFamilias_RetornaVacio()
    {
        var repo = new InMemoryFamilyRepository();

        var result = await new ListFamilyService(repo, TenantFakes.NoFilter(new InMemoryPersonProfileReader()), new InMemoryPersonProfileReader()).ExecuteAsync();

        Assert.Empty(result);
    }
}
