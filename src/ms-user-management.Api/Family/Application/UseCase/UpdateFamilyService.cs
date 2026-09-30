using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;

namespace ms_user_management.Api.Family.Application.UseCase;

public class UpdateFamilyService : IUpdateFamilyUseCase
{
    private readonly IFamilyRepository _repository;

    public UpdateFamilyService(IFamilyRepository familyRepository)
    {
        _repository = familyRepository;
    }

    public async Task ExecuteAsync(Guid familyId,
        string familyName,
        string? observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        var existing = await _repository.GetByIdAsync(familyId, ct);
        if (existing is null)
            throw new InvalidOperationException($"Family not found: {familyId}");

        var relations = FamilyRules.Validate(familyName, members);

        var ownProfileIds = (await _repository.GetMembersAsync(familyId, ct))
            .Select(m => m.ProfileId)
            .ToHashSet();

        var associated = await _repository.GetAssociatedProfileIdsAsync(
            relations.Select(x => x.ProfileId), ct);
        var conflicts = associated.Except(ownProfileIds).ToList();
        if (conflicts.Count > 0)
            throw new InvalidOperationException(
                $"Profiles already associated with another family: {string.Join(", ", conflicts)}");

        await _repository.UpdateAsync(familyId, familyName.Trim(),
            observations ?? string.Empty, relations, ct);
    }
}
