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

    public Task<IReadOnlyList<(FamilyModel Family, Guid? ParentProfileId)>> GetAllAsync(
        CancellationToken ct = default)
    {
        IReadOnlyList<(FamilyModel Family, Guid? ParentProfileId)> rows = Families
            .Select(f => (f, Members
                .Where(m => m.FamilyId == f.Id && m.RelationType == RelationType.Parent)
                .Select(m => (Guid?)m.ProfileId)
                .FirstOrDefault()))
            .ToList();
        return Task.FromResult(rows);
    }

    public Task<FamilyModel?> GetByIdAsync(Guid familyId, CancellationToken ct = default)
        => Task.FromResult(Families.FirstOrDefault(f => f.Id == familyId));

    public Task<IReadOnlyList<(Guid ProfileId, RelationType RelationType)>> GetMembersAsync(
        Guid familyId, CancellationToken ct = default)
    {
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members = Members
            .Where(m => m.FamilyId == familyId)
            .Select(m => (m.ProfileId, m.RelationType))
            .ToList();
        return Task.FromResult(members);
    }

    public Task UpdateAsync(Guid familyId, string familyName, string observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        var family = Families.FirstOrDefault(f => f.Id == familyId)
            ?? throw new InvalidOperationException($"Family not found: {familyId}");

        family.Name = familyName;
        family.Observations = observations;

        Members.RemoveAll(m => m.FamilyId == familyId);
        Members.AddRange(members.Select(m => (Guid.NewGuid(), familyId, m.ProfileId, m.RelationType)));

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid familyId, CancellationToken ct = default)
    {
        var family = Families.FirstOrDefault(f => f.Id == familyId)
            ?? throw new InvalidOperationException($"Family not found: {familyId}");

        Families.Remove(family);
        Members.RemoveAll(m => m.FamilyId == familyId);

        return Task.CompletedTask;
    }
}
