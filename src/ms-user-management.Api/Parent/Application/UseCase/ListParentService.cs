using AutoMapper;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class ListParentService : IListParentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public ListParentService(IPersonRepository personRepository, IMapper mapper)
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
