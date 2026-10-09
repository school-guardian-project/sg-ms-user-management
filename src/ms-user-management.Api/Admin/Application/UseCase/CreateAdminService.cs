using AutoMapper;
using ms_user_management.Api.Admin.Domain.Event;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Admin.Application.UseCase;

public class CreateAdminService : ICreateAdminUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISchoolDirectory _schoolDirectory;

    public CreateAdminService(
        IPersonRepository personRepository,
        IMapper mapper,
        IEventPublisher eventPublisher,
        ISchoolDirectory schoolDirectory)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _schoolDirectory = schoolDirectory;
    }

    public async Task CreateAsync(CreateAdminRequestDto dto)
    {
        // Un admin pertenece a un colegio, no a una sede. Sin colegio no puede
        // operar: no sabria que estudiantes ni que rutas son de su colegio. Por
        // eso schoolId es obligatorio y se valida contra ms-school-management
        // antes de guardar nada.
        var schoolId = dto.SchoolId
            ?? throw new SchoolIdRequiredException();

        await _schoolDirectory.EnsureSchoolExistsAsync(schoolId);

        var person = _mapper.Map<Person>(dto);

        person.Id = Guid.NewGuid();
        person.Status = Status.Active;

        await _personRepository.SaveAsync(person);

        var adminCreatedEvent = new AdminCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            Email = person.Email,
            IdentificationNumber = person.IdentificationNumber,
            // CampusId queda null a proposito: es la sede, y un admin no tiene una.
            SchoolId = schoolId
        };

        await _eventPublisher.PublishAsync("admin.created", adminCreatedEvent);
    }
}