using AutoMapper;
using ms_user_management.Api.Parent.Domain.Event;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class CreateParentService : ICreateParentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISchoolDirectory _schoolDirectory;

    public CreateParentService(IPersonRepository personRepository,
        IMapper mapper,
        IEventPublisher eventPublisher,
        ISchoolDirectory schoolDirectory)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _schoolDirectory = schoolDirectory;
    }

    public async Task CreateAsync(CreatePersonRequestDto dto)
    {
        // La sede se valida antes de escribir. Un id que no corresponde a
        // una sede activa es un dato invalido del formulario: la respuesta
        // correcta es 400, no un registro guardado sin sede.
        var campusId = dto.CampusId
            ?? throw new CampusIdRequiredException("parent");

        await _schoolDirectory.EnsureCampusExistsAsync(campusId);

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
            CampusId = campusId
        };
        
        await _eventPublisher.PublishAsync("parent.created", parentCreatedEvent);
    }
}
