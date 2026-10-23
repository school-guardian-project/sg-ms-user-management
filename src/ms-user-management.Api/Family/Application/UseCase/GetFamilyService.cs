using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Application.Mapper;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class GetFamilyService : IGetFamilyUseCase
{
    private readonly IFamilyRepository _repository;

    public GetFamilyService(IFamilyRepository familyRepository)
    {
        _repository = familyRepository;
    }

    public async Task<FamilyResponseDto> ExecuteAsync(Guid familyId, CancellationToken ct = default)
    {
        var family = await _repository.GetByIdAsync(familyId, ct);
        if (family is null)
            throw new InvalidOperationException($"Family not found: {familyId}");

        var members = await _repository.GetMembersAsync(familyId, ct);

        return FamilyMapper.ToFamilyResponse(family, members);
    }
}
