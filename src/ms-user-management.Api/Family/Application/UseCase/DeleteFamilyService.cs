using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class DeleteFamilyService : IDeleteFamilyUseCase
{
    private readonly IFamilyRepository _repository;

    public DeleteFamilyService(IFamilyRepository familyRepository)
    {
        _repository = familyRepository;
    }

    public async Task ExecuteAsync(Guid familyId, CancellationToken ct = default)
    {
        await _repository.DeleteAsync(familyId, ct);
    }
}
