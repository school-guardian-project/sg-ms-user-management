using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class UserCampusCreationTests
{
    private sealed class SchoolDirectory(Guid campusId) : ISchoolDirectory
    {
        public Task<string?> FindSchoolNameAsync(Guid schoolId, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task EnsureSchoolExistsAsync(Guid schoolId, CancellationToken ct = default) =>
            throw new NotSupportedException();
        public Task<string?> FindCampusNameAsync(Guid id, CancellationToken ct = default) =>
            Task.FromResult<string?>(id == campusId ? "Sede central" : null);
        public Task EnsureCampusExistsAsync(Guid id, CancellationToken ct = default) =>
            id == campusId ? Task.CompletedTask : throw new CampusNotFoundException(id);
    }
    private sealed class Publisher : IEventPublisher
    {
        public string? Topic { get; private set; }
        public object? Event { get; private set; }
        public Task PublishAsync<T>(string topic, T @event)
        {
            Topic = topic;
            Event = @event;
            return Task.CompletedTask;
        }
    }

    private static IMapper Mapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PersonProfile>());
        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    [TestCase("student")]
    [TestCase("driver")]
    [TestCase("parent")]
    public async Task CreationPublishesSelectedCampusAndRejectsMissingOrUnknownCampus(string role)
    {
        var repository = new InMemoryPersonRepository();
        var publisher = new Publisher();
        var campusId = Guid.NewGuid();
        var schools = new SchoolDirectory(campusId);
        var mapper = Mapper();
        Func<CreatePersonRequestDto, Task> create = role switch
        {
            "student" => new CreateStudentService(repository, mapper, publisher, schools).CreateAsync,
            "driver" => new CreateDriverService(repository, mapper, publisher, schools).CreateAsync,
            _ => new CreateParentService(repository, mapper, publisher, schools).CreateAsync
        };
        var dto = new CreatePersonRequestDto
        {
            Name = "Campus", LastName = "Test", Email = "campus@example.invalid",
            IdentificationType = "CC", IdentificationNumber = "123456",
            Phone = 3001234567, ResidenceAddress = "Test street",
            DateBirth = new DateOnly(2000, 1, 1)
        };
        Assert.ThrowsAsync<CampusIdRequiredException>(() => create(dto));
        dto.CampusId = Guid.NewGuid();
        Assert.ThrowsAsync<CampusNotFoundException>(() => create(dto));
        Assert.That(await repository.GetAllAsync(), Is.Empty);
        Assert.That(publisher.Event, Is.Null);

        dto.CampusId = campusId;
        await create(dto);
        Assert.That(await repository.GetAllAsync(), Has.Exactly(1).Items);
        Assert.That(publisher.Topic, Is.EqualTo($"{role}.created"));
        Assert.That(publisher.Event, Is.Not.Null);
        Assert.That(publisher.Event.GetType().GetProperty("CampusId")!.GetValue(publisher.Event), Is.EqualTo(campusId));
    }
}
