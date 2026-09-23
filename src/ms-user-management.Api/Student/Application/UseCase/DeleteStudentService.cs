using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Student.Domain.Ports.In;

namespace ms_user_management.Api.Student.Application.UseCase;

public class DeleteStudentService : IDeleteStudentUseCase
{
    private readonly IPersonRepository _personRepository;

    public DeleteStudentService(IPersonRepository personRepository)
    {
        _personRepository = personRepository;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _personRepository.DeleteAsync(id);
    }
}