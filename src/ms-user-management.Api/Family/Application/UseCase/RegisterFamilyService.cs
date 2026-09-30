using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Family.Domain.Ports.Out;
using ms_user_management.Api.Shared.Domain.Model;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Api.Family.Application.UseCase;

public class RegisterFamilyService : IRegisterFamilyUseCase
{
    private readonly IFamilyRepository _repository;

    public RegisterFamilyService(IFamilyRepository familyRepository)
    {
        _repository = familyRepository;
    }

    public async Task<(Guid FamilyId, IReadOnlyList<(Guid MemberId, Guid ProfileId)> Members)> ExecuteAsync(
        string familyName,
        string? observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        var relations = FamilyRules.Validate(familyName, members);

        var associated = await _repository.GetAssociatedProfileIdsAsync(
            relations.Select(x => x.ProfileId), ct);
        if (associated.Count > 0)
            throw new InvalidOperationException(
                $"Profiles already associated with another family: {string.Join(", ", associated)}");

        var family = new FamilyModel
        {
            Id = Guid.NewGuid(),
            Name = familyName.Trim(),
            Observations = observations ?? string.Empty,
            Status = Status.Active
        };

        var memberIds = await _repository.SaveAsync(family, relations, ct);

        return (family.Id, memberIds.Zip(relations,
            (memberId, r) => (MemberId: memberId, ProfileId: r.ProfileId)).ToList());
    }
}
