using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Tests.Fakes;

public class InMemoryPersonProfileReader : IPersonProfileReader
{
    public readonly Dictionary<Guid, Guid> ProfileByPerson = new();
    public readonly Dictionary<Guid, RoleId> RoleByPerson = new();
    public readonly Dictionary<Guid, string> NameByProfile = new();
    public readonly Dictionary<Guid, Guid> CampusByPerson = new();
    public readonly Dictionary<Guid, Guid> CampusByProfile = new();
    public readonly Dictionary<Guid, Guid> SchoolByCampus = new();

    public Task<IReadOnlyDictionary<Guid, Guid>> GetProfileIdsByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, Guid> result = ProfileByPerson
            .Where(kv => personIds.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return Task.FromResult(result);
    }

    public Task<IReadOnlySet<Guid>> GetPersonIdsByRoleAsync(
        RoleId roleId,
        IReadOnlyCollection<Guid>? campusIds = null,
        CancellationToken ct = default)
    {
        IReadOnlySet<Guid> result = RoleByPerson
            .Where(kv => kv.Value == roleId)
            .Where(kv => campusIds is null
                || (CampusByPerson.TryGetValue(kv.Key, out var campus) && campusIds.Contains(campus)))
            .Select(kv => kv.Key)
            .ToHashSet();

        return Task.FromResult(result);
    }

    public Task<IReadOnlySet<Guid>> GetProfileIdsInCampusesAsync(
        IReadOnlyCollection<Guid> campusIds,
        CancellationToken ct = default)
    {
        IReadOnlySet<Guid> result = CampusByProfile
            .Where(kv => campusIds.Contains(kv.Value))
            .Select(kv => kv.Key)
            .ToHashSet();

        return Task.FromResult(result);
    }

    public Task<bool> ProfileExistsAsync(Guid profileId, CancellationToken ct = default)
        => Task.FromResult(ProfileByPerson.ContainsValue(profileId) || NameByProfile.ContainsKey(profileId));

    public Task<string?> GetPersonNameAsync(Guid profileId, CancellationToken ct = default)
        => Task.FromResult(NameByProfile.TryGetValue(profileId, out var name) ? name : null);

    public Task<Guid?> GetSchoolIdByCampusIdAsync(Guid campusId, CancellationToken ct = default)
        => Task.FromResult<Guid?>(SchoolByCampus.TryGetValue(campusId, out var schoolId) ? schoolId : null);
}
