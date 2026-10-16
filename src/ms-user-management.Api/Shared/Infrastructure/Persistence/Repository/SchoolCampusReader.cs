using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

public class SchoolCampusReader : ISchoolCampusReader
{
    private readonly UserManagementContext _context;

    public SchoolCampusReader(UserManagementContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Guid>> GetCampusIdsBySchoolAsync(Guid schoolId, CancellationToken ct = default)
    {
        return await _context.Set<SchoolCampusRefEntity>()
            .AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .Select(c => c.Id)
            .ToListAsync(ct);
    }
}
