using AutoMapper;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class ListParentService : IListParentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonProfileReader _profileReader;
    private readonly IMapper _mapper;

    public ListParentService(IPersonRepository personRepository, IPersonProfileReader profileReader, IMapper mapper)
    {
        _personRepository = personRepository;
        _profileReader = profileReader;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var parents = await _profileReader.GetPersonIdsByRoleAsync(RoleId.Parent);
        var persons = (await _personRepository.GetAllAsync())
            .Where(p => parents.Contains(p.Id))
            .ToList();
        var result = _mapper.Map<List<PersonListDto>>(persons);

        var profiles = await _profileReader.GetProfileIdsByPersonIdsAsync(persons.Select(p => p.Id).ToList());
        foreach (var dto in result)
        {
            if (profiles.TryGetValue(dto.Id, out var profileId))
                dto.ProfileId = profileId;
        }

        return result;
    }
}
