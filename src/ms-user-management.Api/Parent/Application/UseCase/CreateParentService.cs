using AutoMapper;
using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class CreateParentService : ICreateParentUseCase
{
    private readonly IPersonRepository _personRepository;
    private readonly IMapper _mapper;

    public CreateParentService(IPersonRepository personRepository, IMapper mapper)
    {
        _personRepository = personRepository;
        _mapper = mapper;
    }

    public async Task CreateAsync(PersonRequestDto dto)
    {
        var person = _mapper.Map<Person>(dto);

        person.Id = Guid.NewGuid();
        person.Status = Status.Active;
        
        await _personRepository.SaveAsync(person);
    }
}
