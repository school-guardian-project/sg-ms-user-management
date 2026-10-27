using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;

namespace ms_user_management.Api.Family.Infrastructure.Repository;

public class FamilyMembershipReader : IFamilyMembershipReader
{
    private readonly UserManagementContext _context;

    public FamilyMembershipReader(UserManagementContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlySet<Guid>> GetFamilyIdsByMemberAsync(Guid profileId, CancellationToken ct = default)
    {
        var familyIds = await _context.FamilyMembers
            .AsNoTracking()
            .Where(fm => fm.ProfileId == profileId)
            .Select(fm => fm.FamilyId)
            .Distinct()
            .ToListAsync(ct);

        return familyIds.ToHashSet();
    }

    public async Task<IReadOnlySet<Guid>> GetMemberProfileIdsAsync(
        IReadOnlyCollection<Guid> familyIds, CancellationToken ct = default)
    {
        if (familyIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var ids = familyIds.ToList();
        var profileIds = await _context.FamilyMembers
            .AsNoTracking()
            .Where(fm => ids.Contains(fm.FamilyId))
            .Select(fm => fm.ProfileId)
            .Distinct()
            .ToListAsync(ct);

        return profileIds.ToHashSet();
    }
}
