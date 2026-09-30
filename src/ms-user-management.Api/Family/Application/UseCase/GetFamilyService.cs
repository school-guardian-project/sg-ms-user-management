using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Domain.Model;
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

        return new FamilyResponseDto
        {
            Id = family.Id,
            Name = family.Name,
            ParentProfileId = members
                .Where(m => m.RelationType == RelationType.Parent)
                .Select(m => (Guid?)m.ProfileId)
                .FirstOrDefault(),
            Children = members
                .Where(m => m.RelationType == RelationType.Student)
                .Select(m => m.ProfileId)
                .ToList(),
            Description = family.Observations
        };
    }
}
