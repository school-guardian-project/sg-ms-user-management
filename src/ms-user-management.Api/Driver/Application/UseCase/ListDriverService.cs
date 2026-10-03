using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class ListDriverService : IListDriverUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonLicenseReader _licenseReader;
    private readonly IMapper _mapper;

    public ListDriverService(IPersonRepository personRepository, IPersonLicenseReader licenseReader, IMapper mapper)
    {
        _personRepository = personRepository;
        _licenseReader = licenseReader;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var persons = (await _personRepository.GetAllAsync()).ToList();
        var result = _mapper.Map<List<PersonListDto>>(persons);

        var licenses = await _licenseReader.GetByPersonIdsAsync(persons.Select(p => p.Id).ToList());
        foreach (var dto in result)
        {
            if (licenses.TryGetValue(dto.Id, out var license))
            {
                dto.LicenseNumber = license.LicenseNumber;
                dto.LicenseExpirationDate = license.LicenseExpirationDate;
            }
        }

        return result;
    }
}
