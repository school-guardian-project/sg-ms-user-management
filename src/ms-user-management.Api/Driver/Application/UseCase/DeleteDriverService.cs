using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class DeleteDriverService : IDeleteDriverUseCase
{
    private readonly IPersonRepository _personRepository;

    public DeleteDriverService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _personRepository.DeleteAsync(id);
    }
}
