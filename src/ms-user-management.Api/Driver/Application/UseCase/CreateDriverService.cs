using AutoMapper;
using ms_user_management.Api.Driver.Domain.Event;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class CreateDriverService : ICreateDriverUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IEventPublisher _eventPublisher;
    private readonly ISchoolDirectory _schoolDirectory;
    private readonly IPersonLicenseWriter? _licenseWriter;

    public CreateDriverService(IPersonRepository personRepository,
        IMapper mapper,
        IEventPublisher eventPublisher,
        ISchoolDirectory schoolDirectory,
        IPersonLicenseWriter? licenseWriter = null)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
        _schoolDirectory = schoolDirectory;
        _licenseWriter = licenseWriter;
    }

    public async Task CreateAsync(CreatePersonRequestDto dto)
    {
        // La sede se valida antes de escribir. Un id que no corresponde a
        // una sede activa es un dato invalido del formulario: la respuesta
        // correcta es 400, no un registro guardado sin sede.
        var campusId = dto.CampusId
            ?? throw new CampusIdRequiredException("driver");

        await _schoolDirectory.EnsureCampusExistsAsync(campusId);

        var hasLicense = _licenseWriter is not null && DriverLicenseSync.HasLicense(dto);
        if (hasLicense)
        {
            DriverLicenseRules.ValidateLicenseNumber(dto.LicenseNumber ?? string.Empty);
            DriverLicenseRules.ValidateExpirationDate(dto.LicenseExpirationDate ?? default);
        }

        var person = _mapper.Map<Person>(dto);

        person.Id = Guid.NewGuid();
        person.Status = Status.Active;
        
        await _personRepository.SaveAsync(person);
        
        var driverCreatedEvent = new DriverCreatedEvent
        {
            EventId = Guid.NewGuid(),
            PersonId = person.Id,
            Email = person.Email,
            IdentificationNumber = person.IdentificationNumber,
            CampusId = campusId
        };
        
        await _eventPublisher.PublishAsync("driver.created", driverCreatedEvent);

        if (hasLicense)
            await SaveLicenseWhenProfileExistsAsync(person.Id, dto);
    }

    /// <summary>
    /// IAM crea el perfil al consumir driver.created, asi que la licencia se
    /// guarda en cuanto el perfil aparece (espera acotada, ~4 s).
    /// </summary>
    private async Task SaveLicenseWhenProfileExistsAsync(Guid personId, PersonRequestDto dto)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            if (await DriverLicenseSync.SaveAsync(_licenseWriter!, personId, dto)) return;
            await Task.Delay(400);
        }
    }
}
