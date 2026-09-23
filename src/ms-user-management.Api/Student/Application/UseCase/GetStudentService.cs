using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.UseCase;

public class GetStudentService : IGetPersonUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public GetStudentService(IPersonRepository personRepository, IMapper mapper)
    {
        _personRepository = personRepository;
        _mapper = mapper;
    }

    public async Task<PersonResponseDto> GetByIdAsync(Guid id)
    {
        var persons = await _personRepository.GetByIdAsync(id);

        return _mapper.Map<PersonResponseDto>(persons);
    }
}