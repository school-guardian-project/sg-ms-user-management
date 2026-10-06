using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class GetDriverService : IGetPersonUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonLicenseReader _licenseReader;
    private readonly IMapper _mapper;

    public GetDriverService(IPersonRepository personRepository, IPersonLicenseReader licenseReader, IMapper mapper)
    {
        _personRepository = personRepository;
        _licenseReader = licenseReader;
        _mapper = mapper;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        var person = await _personRepository.GetByIdAsync(id);
        var dto = _mapper.Map<PersonResponseDto>(person);

        var license = await _licenseReader.GetByPersonIdsAsync(new[] { id });
        if (license.TryGetValue(id, out var found))
        {
            dto.LicenseNumber = found.LicenseNumber;
            dto.LicenseExpirationDate = found.LicenseExpirationDate;
        }

        return dto;
    }
}
