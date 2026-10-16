using ms_user_management.Api.Shared.Domain.Event;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Application.UseCase;

/// <summary>
/// Activa o desactiva la cuenta de una persona y publica
/// <c>user.status.changed</c> para que ms-iam refleje el estado en su
/// <c>Profile</c> (login y asignaciones). Compartido por los cuatro roles.
/// </summary>
public class ChangePersonStatusService
{
    private const string Topic = "user.status.changed";

    private readonly IPersonRepository _personRepository;
    private readonly IEventPublisher _eventPublisher;

    public ChangePersonStatusService(IPersonRepository personRepository, IEventPublisher eventPublisher)
    {
        _personRepository = personRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task ChangeStatusAsync(Guid personId, string status)
    {
        if (!Enum.TryParse<Status>(status, ignoreCase: true, out var parsed))
            throw new InvalidPersonStatusException(status);

        var person = await _personRepository.GetByIdAsync(personId)
            ?? throw new PersonNotFoundException(personId);

        // Idempotente: fijar el mismo estado no es un error, solo se notifica.
        person.Status = parsed;
        await _personRepository.UpdateAsync(person);

        await _eventPublisher.PublishAsync(Topic, new PersonStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            Email = person.Email,
            Status = parsed.ToString()
        });
    }
}
