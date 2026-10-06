using AutoMapper;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class GetParentService : IGetPersonUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public GetParentService(IPersonRepository personRepository, IMapper mapper)
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
