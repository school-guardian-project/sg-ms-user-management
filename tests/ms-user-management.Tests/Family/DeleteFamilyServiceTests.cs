using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

public class DeleteFamilyServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ConFamilia_EliminaFamiliaYMiembros()
    {
        var repo = new InMemoryFamilyRepository();
        var family = new FamilyModel
        {
            Id = Guid.NewGuid(),
            Name = "Perez",
            Observations = "",
            Status = Status.Active
        };
        repo.Families.Add(family);
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Parent));
        repo.Members.Add((Guid.NewGuid(), family.Id, Guid.NewGuid(), RelationType.Student));

        await new DeleteFamilyService(repo).ExecuteAsync(family.Id);

        Assert.Empty(repo.Families);
        Assert.DoesNotContain(repo.Members, m => m.FamilyId == family.Id);
    }

    [Fact]
    public async Task ExecuteAsync_FamiliaInexistente_LanzaError()
    {
        var repo = new InMemoryFamilyRepository();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new DeleteFamilyService(repo).ExecuteAsync(Guid.NewGuid()));

        Assert.Contains("not found", ex.Message);
    }
}
