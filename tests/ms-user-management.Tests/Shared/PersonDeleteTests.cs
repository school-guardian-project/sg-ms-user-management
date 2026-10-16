using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

/// <summary>
/// FR-1.2: dar de baja a una persona. Los cuatro casos de uso (student, driver,
/// parent, admin) delegan en el repositorio con el id tal cual, sin logica
/// propia; la prueba fija ese contrato, que es lo unico que puede romper un
/// cambio en el servicio.
/// </summary>
[TestFixture]
public class PersonDeleteTests
{
    private static Task DeleteAsync(string role, Guid id, IPersonRepository repository) => role switch
    {
        "student" => new DeleteStudentService(repository).DeleteAsync(id),
        "driver" => new DeleteDriverService(repository).DeleteAsync(id),
        "parent" => new DeleteParentService(repository).DeleteAsync(id),
        _ => new DeleteAdminService(repository).DeleteAsync(id)
    };

    [TestCase("student")]
    [TestCase("driver")]
    [TestCase("parent")]
    [TestCase("admin")]
    public async Task Delete_ConPersonaExistente_LePasaElIdAlRepositorio(string role)
    {
        var entityId = Guid.NewGuid();
        var repository = new RecordingPersonRepository();

        await DeleteAsync(role, entityId, repository);

        Assert.That(repository.DeletedId, Is.EqualTo(entityId));
    }

    [TestCase("student")]
    [TestCase("driver")]
    [TestCase("parent")]
    [TestCase("admin")]
    public async Task Delete_ConIdDesconocido_IgualLlamaAlRepositorio(string role)
    {
        // El caso de uso no valida existencia: la baja la resuelve el
        // repositorio. Se fija para no mover esa responsabilidad sin querer.
        var repository = new RecordingPersonRepository();

        await DeleteAsync(role, Guid.NewGuid(), repository);

        Assert.That(repository.DeletedId, Is.Not.Null);
    }
}
