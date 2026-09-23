using ms_user_management.Api.Parent.Domain.Ports.In;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Parent.Application.UseCase;

public class DeleteParentService : IDeleteParentUseCase
{
    private readonly IPersonRepository _personRepository;

    public DeleteParentService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _personRepository.DeleteAsync(id);
    }
}
