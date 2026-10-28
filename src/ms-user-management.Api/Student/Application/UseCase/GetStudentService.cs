using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.UseCase;

public class GetStudentService : IGetPersonUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;
    private readonly IPersonProfileReader _profileReader;

    public GetStudentService(IPersonRepository personRepository, IMapper mapper, IPersonProfileReader profileReader)
    {
        _personRepository = personRepository;
        _mapper = mapper;
        _profileReader = profileReader;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        var persons = await _personRepository.GetByIdAsync(id);

        var result = _mapper.Map<PersonResponseDto>(persons);
        var profiles = await _profileReader.GetProfileIdsByPersonIdsAsync(new[] { persons.Id });
        if (profiles.TryGetValue(persons.Id, out var profileId))
            result.ProfileId = profileId;

        return result;
    }
}