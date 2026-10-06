using AutoMapper;
using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Admin.Application.UseCase;

public class GetAdminService : IGetPersonUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public GetAdminService(IPersonRepository personRepository, IMapper mapper)
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
