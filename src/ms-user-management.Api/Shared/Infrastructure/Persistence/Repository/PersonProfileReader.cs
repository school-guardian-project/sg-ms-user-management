using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

public class PersonProfileReader : IPersonProfileReader
{
    private readonly UserManagementContext _context;

    public PersonProfileReader(UserManagementContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<Guid, Guid>> GetProfileIdsByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default)
    {
        var ids = personIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, Guid>();

        var profiles = await _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => ids.Contains(p.PersonId))
            .Select(p => new { p.Id, p.PersonId })
            .ToListAsync(ct);

        var result = new Dictionary<Guid, Guid>();
        foreach (var profile in profiles)
            result[profile.PersonId] = profile.Id;

        return result;
    }

    public async Task<IReadOnlySet<Guid>> GetPersonIdsByRoleAsync(
        RoleId roleId,
        IReadOnlyCollection<Guid>? campusIds = null,
        CancellationToken ct = default)
    {
        var query = _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => p.RoleId == roleId);

        if (campusIds is not null)
            query = query.Where(p => p.CampuseId != null && campusIds.Contains(p.CampuseId.Value));

        var personIds = await query
            .Select(p => p.PersonId)
            .ToListAsync(ct);

        // HashSet: los listados hacen Contains por cada persona.
        return personIds.ToHashSet();
    }

    public async Task<IReadOnlySet<Guid>> GetProfileIdsInCampusesAsync(
        IReadOnlyCollection<Guid> campusIds,
        CancellationToken ct = default)
    {
        if (campusIds.Count == 0)
            return new HashSet<Guid>();

        var profileIds = await _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => p.CampuseId != null && campusIds.Contains(p.CampuseId.Value))
            .Select(p => p.Id)
            .ToListAsync(ct);

        return profileIds.ToHashSet();
    }

    public Task<bool> ProfileExistsAsync(Guid profileId, CancellationToken ct = default)
    {
        return _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .AnyAsync(p => p.Id == profileId, ct);
    }

    public async Task<string?> GetPersonNameAsync(Guid profileId, CancellationToken ct = default)
    {
        var personId = await _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => p.Id == profileId)
            .Select(p => (Guid?)p.PersonId)
            .FirstOrDefaultAsync(ct);

        if (personId is null)
            return null;

        var person = await _context.Person
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == personId.Value, ct);

        return person is null
            ? null
            : $"{person.Name} {person.LastName}".Trim();
    }
}
