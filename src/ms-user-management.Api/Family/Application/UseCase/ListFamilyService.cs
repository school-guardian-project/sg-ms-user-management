using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class ListFamilyService : IListFamilyUseCase
{
    private readonly IFamilyRepository _repository;

    public ListFamilyService(IFamilyRepository familyRepository)
    {
        _repository = familyRepository;
    }

    public async Task<IEnumerable<FamilyListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var families = await _repository.GetAllAsync(ct);

        return families.Select(f => new FamilyListDto
        {
            Id = f.Family.Id,
            Name = f.Family.Name,
            ParentProfileId = f.ParentProfileId
        }).ToList();
    }
}
