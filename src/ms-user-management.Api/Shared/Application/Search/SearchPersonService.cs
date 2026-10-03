using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

namespace ms_user_management.Api.Shared.Application.Search;

public class SearchPersonService
{
    private readonly IEnumerable<IPersonSearchStrategy> _strategies;
    private readonly IPersonLicenseReader _licenseReader;
    private readonly IMapper _mapper;

    public SearchPersonService(IEnumerable<IPersonSearchStrategy> strategies, IPersonLicenseReader licenseReader, IMapper mapper)
    {
        _strategies = strategies;
        _licenseReader = licenseReader;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> SearchAsync(string search)
    {
        search = search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(search)) return [];

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy == null) return [];
        
        var persons = (await strategy.SearchAsync(search)).ToList();
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