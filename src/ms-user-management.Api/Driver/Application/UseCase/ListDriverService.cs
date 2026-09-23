using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class ListDriverService : IListDriverUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public ListDriverService(IPersonRepository personRepository, IMapper mapper)
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
