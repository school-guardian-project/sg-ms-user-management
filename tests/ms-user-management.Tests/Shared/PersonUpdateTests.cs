using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

/// <summary>
/// FR-1.2: editar una persona ya registrada (el <c>PUT</c> de student, driver,
/// parent y admin). Los cuatro casos de uso tienen la misma forma — buscan por
/// id, si no esta tiran, mapean el DTO sobre la entidad y la guardan — y admin
/// ademas emite <c>admin.school.updated</c>. Hasta ahora esa familia no tenia
/// ninguna prueba.
/// </summary>
[TestFixture]
public class PersonUpdateTests
{
    private static IMapper Mapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PersonProfile>());
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static Person Stored() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Ana",
        LastName = "Perez",
        IdentificationType = IdentificationType.TI,
        IdentificationNumber = "1001",
        Email = "ana@example.invalid",
        Phone = 3001112222,
        ResidenceAddress = "Calle 1",
        DateBirth = new DateOnly(2005, 3, 10),
        Status = Status.Active
    };

    private static PersonRequestDto NewData() => new()
    {
        Name = "Ana Maria",
        LastName = "Perez Gomez",
        IdentificationType = "CC",
        IdentificationNumber = "1001",
        Email = "ana.maria@example.invalid",
        Phone = 3009998888,
        ResidenceAddress = "Carrera 9",
        DateBirth = new DateOnly(2005, 3, 10)
    };

    private static Task UpdateAsync(string role, Guid id, PersonRequestDto dto,
        IPersonRepository repository, IMapper mapper, IEventPublisher publisher) => role switch
    {
        "student" => new UpdateStudentService(repository, mapper).UpdateAsync(id, dto),
        "driver" => new UpdateDriverService(repository, mapper).UpdateAsync(id, dto),
        "parent" => new UpdateParentService(repository, mapper).UpdateAsync(id, dto),
        _ => new UpdateAdminService(repository, mapper, publisher).UpdateAsync(id, dto)
    };

    private sealed class Publisher : IEventPublisher
    {
        public readonly List<(string Topic, object Event)> Published = new();

        public Task PublishAsync<T>(string topic, T @event)
        {
            Published.Add((topic, @event!));
            return Task.CompletedTask;
        }
    }

    [TestCase("student")]
    [TestCase("driver")]
    [TestCase("parent")]
    [TestCase("admin")]
    public async Task Update_ConPersonaExistente_MapeaElDtoSobreLaEntidadYLaGuarda(string role)
    {
        var person = Stored();
        var repository = new RecordingPersonRepository(person);

        await UpdateAsync(role, person.Id, NewData(), repository, Mapper(), new Publisher());

        Assert.That(repository.Updated, Is.SameAs(person));
        Assert.That(person.Name, Is.EqualTo("Ana Maria"));
        Assert.That(person.LastName, Is.EqualTo("Perez Gomez"));
        Assert.That(person.Email, Is.EqualTo("ana.maria@example.invalid"));
        Assert.That(person.Phone, Is.EqualTo(3009998888));
        Assert.That(person.ResidenceAddress, Is.EqualTo("Carrera 9"));
        Assert.That(person.IdentificationType, Is.EqualTo(IdentificationType.CC));
    }

    [TestCase("student")]
    [TestCase("driver")]
    [TestCase("parent")]
    [TestCase("admin")]
    public async Task Update_ConIdDesconocido_LanzaNotFoundYNoGuarda(string role)
    {
        var repository = new RecordingPersonRepository();

        var ex = Assert.ThrowsAsync<Exception>(
            () => UpdateAsync(role, Guid.NewGuid(), NewData(), repository, Mapper(), new Publisher()));

        Assert.That(ex!.Message, Is.EqualTo("Not Found"));
        Assert.That(repository.Updated, Is.Null);
    }

    [Test]
    public async Task Update_ComoAdmin_EmiteAdminSchoolUpdatedConElColegio()
    {
        var person = Stored();
        person.SchoolId = Guid.NewGuid();
        var repository = new RecordingPersonRepository(person);
        var publisher = new Publisher();

        await new UpdateAdminService(repository, Mapper(), publisher)
            .UpdateAsync(person.Id, NewData());

        Assert.That(publisher.Published, Has.Count.EqualTo(1));
        Assert.That(publisher.Published[0].Topic, Is.EqualTo("admin.school.updated"));
    }
}
