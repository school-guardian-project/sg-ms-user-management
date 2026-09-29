using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.Out;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Tests.Fakes;

public class InMemoryFamilyRepository : IFamilyRepository
{
    public readonly List<FamilyModel> Families = new();
    public readonly List<(Guid MemberId, Guid FamilyId, Guid ProfileId, RelationType RelationType)> Members = new();

    public Task<IReadOnlyList<Guid>> SaveAsync(FamilyModel family,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        Families.Add(family);
        var ids = members.Select(m =>
        {
            var id = Guid.NewGuid();
            Members.Add((id, family.Id, m.ProfileId, m.RelationType));
            return id;
        }).ToList();
        return Task.FromResult<IReadOnlyList<Guid>>(ids);
    }

    public Task<IReadOnlyList<Guid>> GetAssociatedProfileIdsAsync(
        IEnumerable<Guid> profileIds, CancellationToken ct = default)
    {
        var ids = profileIds.ToHashSet();
        IReadOnlyList<Guid> found = Members
            .Where(m => ids.Contains(m.ProfileId))
            .Select(m => m.ProfileId)
            .Distinct()
            .ToList();
        return Task.FromResult(found);
    }
}
