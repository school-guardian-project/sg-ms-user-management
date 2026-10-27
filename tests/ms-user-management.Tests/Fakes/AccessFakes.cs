using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Tests.Fakes;

/// <summary>Usuario actual configurable; por defecto, sin autenticar.</summary>
public class FakeCurrentUser : ICurrentUser
{
    public bool IsAuthenticated { get; set; }
    public Guid? ProfileId { get; set; }
    public Guid? PersonId { get; set; }
    public RoleId? RoleId { get; set; }
    public HashSet<string> Permissions { get; } = new();

    public bool HasPermission(string permission) => IsAuthenticated && Permissions.Contains(permission);
}

public class InMemoryFamilyMembershipReader : IFamilyMembershipReader
{
    public readonly List<(Guid FamilyId, Guid ProfileId)> Members = new();

    public Task<IReadOnlySet<Guid>> GetFamilyIdsByMemberAsync(Guid profileId, CancellationToken ct = default)
    {
        IReadOnlySet<Guid> result = Members.Where(m => m.ProfileId == profileId).Select(m => m.FamilyId).ToHashSet();
        return Task.FromResult(result);
    }

    public Task<IReadOnlySet<Guid>> GetMemberProfileIdsAsync(IReadOnlyCollection<Guid> familyIds, CancellationToken ct = default)
    {
        IReadOnlySet<Guid> result = Members.Where(m => familyIds.Contains(m.FamilyId)).Select(m => m.ProfileId).ToHashSet();
        return Task.FromResult(result);
    }
}
