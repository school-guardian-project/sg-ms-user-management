using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Domain.Event;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.UseCase;

public class CreateStudentService : ICreateStudentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISchoolDirectory _schoolDirectory;

    public CreateStudentService(
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

    public async Task CreateAsync(CreatePersonRequestDto dto)
    {
        // La sede se valida antes de escribir. Si el id no corresponde a una sede
        // activa, es un dato invalido del formulario y la respuesta correcta es
        // 400: crear el estudiante y dejarlo sin sede seria un registro que no
        // sirve para asignarle una ruta.
        var campusId = dto.CampusId
            ?? throw new CampusIdRequiredException("student");

        await _schoolDirectory.EnsureCampusExistsAsync(campusId);

        var person = _mapper.Map<Person>(dto);

        person.Id = Guid.NewGuid();
        person.Status = Status.Active;

        await _personRepository.SaveAsync(person);

        var studentCreatedEvent = new StudentCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            Email = person.Email,
            IdentificationNumber = person.IdentificationNumber,
            CampusId = campusId
        };

        await _eventPublisher.PublishAsync("student.created", studentCreatedEvent);
    }
}