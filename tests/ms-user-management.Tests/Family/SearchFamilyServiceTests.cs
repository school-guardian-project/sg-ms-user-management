using ms_user_management.Api.Family.Application.Search.Strategy;
using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

public class SearchFamilyServiceTests
{
    private static FamilyModel NewFamily(string name, string observations = "") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        Observations = observations,
        Status = Status.Active
    };

    private static SearchFamilyService NewService(InMemoryFamilyRepository repo) =>
        new(repo, new IFamilySearchStrategy[]
        {
            new PhoneSearchStrategy(),
            new NameSearchStrategy()
        });

    [Fact]
    public async Task SearchAsync_TerminoVacio_RetornaVacio()
    {
        var repo = new InMemoryFamilyRepository();
        repo.Families.Add(NewFamily("Perez"));

        var result = await NewService(repo).SearchAsync("   ");

        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchAsync_NombreDeFamilia_RetornaSoloCoincidencia()
    {
        var repo = new InMemoryFamilyRepository();
        repo.Families.Add(NewFamily("Perez"));
        repo.Families.Add(NewFamily("Gomez"));

        var result = (await NewService(repo).SearchAsync("perez")).ToList();

        var single = Assert.Single(result);
        Assert.Equal("Perez", single.Name);
    }

    [Fact]
    public async Task SearchAsync_NombreDelRepresentante_RetornaLaFamilia()
    {
        var repo = new InMemoryFamilyRepository();
        var family = NewFamily("Perez");
        var profile = Guid.NewGuid();
        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, profile, RelationType.Parent));
        repo.GuardiansByProfile[profile] = ("Maria", "Garcia", 300111222);

        var result = (await NewService(repo).SearchAsync("maria")).ToList();

        var single = Assert.Single(result);
        Assert.Equal(family.Id, single.Id);
        Assert.Equal(profile, single.ParentProfileId);
    }

    [Fact]
    public async Task SearchAsync_SoloDigitos_RetornaPorTelefonoDelRepresentante()
    {
        var repo = new InMemoryFamilyRepository();
        var withPhone = NewFamily("Perez");
        var profile = Guid.NewGuid();
        repo.Families.Add(withPhone);
        repo.Families.Add(NewFamily("Gomez"));
        repo.Members.Add((Guid.NewGuid(), withPhone.Id, profile, RelationType.Parent));
        repo.GuardiansByProfile[profile] = ("Maria", "Garcia", 300111222);

        var result = (await NewService(repo).SearchAsync("111222")).ToList();

        var single = Assert.Single(result);
        Assert.Equal(withPhone.Id, single.Id);
    }
}
