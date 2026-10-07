using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class ListDriverService : IListDriverUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonLicenseReader _licenseReader;
    private readonly IPersonProfileReader _profileReader;
    private readonly IMapper _mapper;

    public ListDriverService(
        IPersonRepository personRepository,
        IPersonLicenseReader licenseReader,
        IPersonProfileReader profileReader,
        IMapper mapper)
    {
        _personRepository = personRepository;
        _licenseReader = licenseReader;
        _profileReader = profileReader;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var drivers = await _profileReader.GetPersonIdsByRoleAsync(RoleId.Driver);
        var persons = (await _personRepository.GetAllAsync())
            .Where(p => drivers.Contains(p.Id))
            .ToList();
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

        var profiles = await _profileReader.GetProfileIdsByPersonIdsAsync(persons.Select(p => p.Id).ToList());
        foreach (var dto in result)
        {
            if (profiles.TryGetValue(dto.Id, out var profileId))
                dto.ProfileId = profileId;
        }

        return result;
    }
}
