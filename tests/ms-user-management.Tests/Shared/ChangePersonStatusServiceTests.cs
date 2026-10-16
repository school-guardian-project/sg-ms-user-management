using ms_user_management.Api.Shared.Application.UseCase;
using ms_user_management.Api.Shared.Domain.Event;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

[TestFixture]
public class ChangePersonStatusServiceTests
{
    private sealed class RecordingPublisher : IEventPublisher
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

    private static Person NewPerson(Status status = Status.Active) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Ana",
        LastName = "Perez",
        Email = "ana@example.com",
        Status = status
    };

    [Test]
    public async Task DeactivatesPersonAndPublishesEvent()
    {
        var person = NewPerson();
        var publisher = new RecordingPublisher();
        var service = new ChangePersonStatusService(new InMemoryPersonRepository(person), publisher);

        await service.ChangeStatusAsync(person.Id, "Inactive");

        Assert.That(person.Status, Is.EqualTo(Status.Inactive));
        Assert.That(publisher.Topic, Is.EqualTo("user.status.changed"));
        var published = publisher.Event as PersonStatusChangedEvent;
        Assert.That(published, Is.Not.Null);
        Assert.That(published!.PersonId, Is.EqualTo(person.Id));
        Assert.That(published.Status, Is.EqualTo("Inactive"));
        Assert.That(published.Email, Is.EqualTo("ana@example.com"));
    }

    [Test]
    public async Task AcceptsStatusCaseInsensitivelyAndIsIdempotent()
    {
        var person = NewPerson(Status.Active);
        var publisher = new RecordingPublisher();
        var service = new ChangePersonStatusService(new InMemoryPersonRepository(person), publisher);

        await service.ChangeStatusAsync(person.Id, "active");

        Assert.That(person.Status, Is.EqualTo(Status.Active));
        Assert.That(publisher.Topic, Is.EqualTo("user.status.changed"));
    }

    [Test]
    public void RejectsUnknownStatus()
    {
        var person = NewPerson();
        var service = new ChangePersonStatusService(
            new InMemoryPersonRepository(person), new RecordingPublisher());

        Assert.ThrowsAsync<InvalidPersonStatusException>(
            () => service.ChangeStatusAsync(person.Id, "Deleted"));
    }
}
