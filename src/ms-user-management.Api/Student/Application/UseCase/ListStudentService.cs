using AutoMapper;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.UseCase;

public class ListStudentService : IListStudentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public ListStudentService(IPersonRepository personRepository, IMapper mapper)
    {
        _personRepository = personRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PersonListDto>> ExecuteAsync()
    {
        var persons = await _personRepository.GetAllAsync();

        return _mapper.Map<IEnumerable<PersonListDto>>(persons);
    }
}