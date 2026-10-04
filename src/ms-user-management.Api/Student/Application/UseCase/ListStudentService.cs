using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.UseCase;

public class ListStudentService : IListStudentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IPersonProfileReader _profileReader;
    private readonly IMapper _mapper;

    public ListStudentService(IPersonRepository personRepository, IPersonProfileReader profileReader, IMapper mapper)
    {
        _personRepository = personRepository;
        _profileReader = profileReader;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var persons = (await _personRepository.GetAllAsync()).ToList();
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
