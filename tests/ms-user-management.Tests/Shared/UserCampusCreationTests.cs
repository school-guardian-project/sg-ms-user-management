using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Tests.Fakes;
using Xunit;

namespace ms_user_management.Tests.Shared;

public class UserCampusCreationTests
{
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

    [Theory]
    [InlineData("student")]
    [InlineData("driver")]
    [InlineData("parent")]
    public async Task CreationPublishesSelectedCampusAndRejectsMissingOrUnknownCampus(string role)
    {
        var repository = new InMemoryPersonRepository();
        var profiles = new InMemoryPersonProfileReader();
        var publisher = new Publisher();
        var campusId = Guid.NewGuid();
        profiles.SchoolByCampus[campusId] = Guid.NewGuid();
        var mapper = Mapper();
        Func<PersonRequestDto, Task> create = role switch
        {
            "student" => new CreateStudentService(repository, mapper, publisher, profiles).CreateAsync,
            "driver" => new CreateDriverService(repository, mapper, publisher, profiles).CreateAsync,
            _ => new CreateParentService(repository, mapper, publisher, profiles).CreateAsync
        };
        var dto = new PersonRequestDto
        {
            Name = "Campus", LastName = "Test", Email = "campus@example.invalid",
            IdentificationType = "CC", IdentificationNumber = "123456",
            Phone = 3001234567, ResidenceAddress = "Test street",
            DateBirth = new DateOnly(2000, 1, 1)
        };
        await Assert.ThrowsAsync<ArgumentException>(() => create(dto));
        dto.CampusId = Guid.NewGuid();
        await Assert.ThrowsAsync<ArgumentException>(() => create(dto));
        Assert.Empty(await repository.GetAllAsync());
        Assert.Null(publisher.Event);

        dto.CampusId = campusId;
        await create(dto);
        Assert.Single(await repository.GetAllAsync());
        Assert.Equal($"{role}.created", publisher.Topic);
        Assert.NotNull(publisher.Event);
        Assert.Equal(campusId, publisher.Event.GetType().GetProperty("CampusId")!.GetValue(publisher.Event));
    }
}
