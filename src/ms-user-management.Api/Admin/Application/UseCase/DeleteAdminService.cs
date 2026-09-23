using ms_user_management.Api.Admin.Domain.Ports.In;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Admin.Application.UseCase;

public class DeleteAdminService : IDeleteAdminUseCase
{
    private readonly IPersonRepository _personRepository;

    public DeleteAdminService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _personRepository.DeleteAsync(id);
    }
}
