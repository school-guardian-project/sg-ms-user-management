using AutoMapper;
using ms_user_management.Api.Parent.Domain.Event;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class CreateParentService : ICreateParentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly IPersonProfileReader _profiles;

    public CreateParentService(IPersonRepository personRepository, IMapper mapper, IEventPublisher eventPublisher, IPersonProfileReader profiles)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _profiles = profiles;
    }

    public async Task CreateAsync(PersonRequestDto dto)
    {
        if (dto.CampusId is not Guid campusId || campusId == Guid.Empty
            || await _profiles.GetSchoolIdByCampusIdAsync(campusId) is null)
            throw new ArgumentException("CampusId must reference an active campus of an active school.");

        var person = _mapper.Map<Person>(dto);

        person.Id = Guid.NewGuid();
        person.Status = Status.Active;
        
        await _personRepository.SaveAsync(person);
        
        var parentCreatedEvent = new ParentCreatedEvent()
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            Email = person.Email,
            IdentificationNumber = person.IdentificationNumber,
            CampusId = dto.CampusId
        };
        
        await _eventPublisher.PublishAsync("parent.created", parentCreatedEvent);
    }
}
