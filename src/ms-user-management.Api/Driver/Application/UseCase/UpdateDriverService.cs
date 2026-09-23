using AutoMapper;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class UpdateDriverService : IUpdateDriverUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public UpdateDriverService(IPersonRepository personRepository, IMapper mapper)
    {
        _personRepository = personRepository;
        _mapper = mapper;
    }

    public async Task UpdateAsync(Guid id, PersonRequestDto dto)
    {
        var person = await _personRepository.GetByIdAsync(id);

        if (person is null) throw new Exception("Not Found");

        _mapper.Map(dto, person);

        await _personRepository.UpdateAsync(person);
    }
}
