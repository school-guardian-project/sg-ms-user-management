using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

namespace ms_user_management.Api.Shared.Application.Search;

public class SearchPersonService
{
    private readonly IEnumerable<IPersonSearchStrategy> _strategies;
    private readonly IPersonLicenseReader _licenseReader;
    private readonly IPersonProfileReader _profileReader;
    private readonly IMapper _mapper;

    public SearchPersonService(
        IEnumerable<IPersonSearchStrategy> strategies,
        IPersonLicenseReader licenseReader,
        IPersonProfileReader profileReader,
        IMapper mapper)
    {
        _strategies = strategies;
        _licenseReader = licenseReader;
        _profileReader = profileReader;
        _mapper = mapper;
    }

    /// <summary>
    /// Busca personas por email, identificación o nombre. Con <paramref name="roleId"/>
    /// devuelve solo las que tienen ese rol: si no, una búsqueda por nombre mezcla
    /// acudientes, estudiantes, conductores y administradores.
    /// </summary>
    public async Task<IEnumerable<PersonListDto>> SearchAsync(string search, RoleId? roleId = null)
    {
        search = search?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(search)) return [];

        var strategy = _strategies.FirstOrDefault(x => x.CanHandle(search));

        if (strategy == null) return [];

        var persons = (await strategy.SearchAsync(search)).ToList();

        if (roleId is not null)
        {
            var allowed = await _profileReader.GetPersonIdsByRoleAsync(roleId.Value);
            persons = persons.Where(p => allowed.Contains(p.Id)).ToList();
        }

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
