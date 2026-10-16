using ms_user_management.Api.Family.Application.UseCase;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Family;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class DeleteFamilyServiceTests
{
    [Test]
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

        Assert.That(repo.Families, Is.Empty);
        Assert.That(repo.Members.Any(m => m.FamilyId == family.Id), Is.False);
    }

    [Test]
    public async Task ExecuteAsync_FamiliaInexistente_LanzaError()
    {
        var repo = new InMemoryFamilyRepository();

        var ex = Assert.ThrowsAsync<InvalidOperationException>(
            () => new DeleteFamilyService(repo).ExecuteAsync(Guid.NewGuid()));

        Assert.That(ex.Message, Does.Contain("not found"));
    }
}
