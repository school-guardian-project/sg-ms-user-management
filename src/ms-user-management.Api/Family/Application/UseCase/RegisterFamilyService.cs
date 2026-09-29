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
        if (string.IsNullOrWhiteSpace(familyName))
            throw new ArgumentException("FamilyName is required.", nameof(familyName));
        if (members is null || members.Count == 0)
            throw new ArgumentException("At least one member is required.", nameof(members));

        var relations = members.Select(m =>
        {
            if (m.ProfileId == Guid.Empty)
                throw new ArgumentException("ProfileId is required for every member.", nameof(members));
            if (!Enum.IsDefined(m.RelationType))
                throw new ArgumentException($"Invalid relationship: {(int)m.RelationType}. Use Parent or Student.");
            return m;
        }).ToList();

        var duplicated = relations.GroupBy(x => x.ProfileId)
            .Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicated.Count > 0)
            throw new InvalidOperationException(
                $"Duplicated profiles in request: {string.Join(", ", duplicated)}");

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
