using AutoMapper;
using ms_user_management.Api.Admin.Domain.Event;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Admin.Application.UseCase;

public class CreateAdminService : ICreateAdminUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;

    public CreateAdminService(IPersonRepository personRepository, IMapper mapper, IEventPublisher eventPublisher)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task CreateAsync(PersonRequestDto dto)
    {
        var person = _mapper.Map<Person>(dto);

        person.Id = Guid.NewGuid();
        person.Status = Status.Active;
        
        await _personRepository.SaveAsync(person);
        
        var adminCreatedEvent = new AdminCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            Email = person.Email,
            IdentificationNumber = person.IdentificationNumber
        };
        
        await _eventPublisher.PublishAsync("admin.created", adminCreatedEvent);
    }
}
