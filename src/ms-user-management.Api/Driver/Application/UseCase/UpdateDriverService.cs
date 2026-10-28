using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class UpdateDriverService : IUpdateDriverUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IPersonLicenseWriter? _licenseWriter;

    public UpdateDriverService(IPersonRepository personRepository, IMapper mapper,
        IPersonLicenseWriter? licenseWriter = null)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _licenseWriter = licenseWriter;
    }

    public async Task UpdateAsync(Guid id, PersonRequestDto dto)
    {
        var person = await _personRepository.GetByIdAsync(id);

        if (person is null) throw new Exception("Not Found");

        if (_licenseWriter is not null && DriverLicenseSync.HasLicense(dto))
        {
            if (!await DriverLicenseSync.SaveAsync(_licenseWriter, id, dto))
                throw new InvalidOperationException("The driver profile is not ready yet; try again in a moment.");
        }

        _mapper.Map(dto, person);

        await _personRepository.UpdateAsync(person);
    }
}
