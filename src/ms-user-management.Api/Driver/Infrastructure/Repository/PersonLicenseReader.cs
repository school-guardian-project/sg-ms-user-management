using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Driver.Infrastructure.Repository;

/// <summary>
/// Lee la licencia del conductor por PersonId: UserManagement.DriverLicense guarda ProfileId,
/// así que se resuelve PersonId -> ProfileId contra Iam.Profile (misma base de datos).
/// </summary>
public class PersonLicenseReader : IPersonLicenseReader
{
    private readonly UserManagementContext _context;

    public PersonLicenseReader(UserManagementContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyDictionary<Guid, DriverLicense>> GetByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default)
    {
        var ids = personIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<Guid, DriverLicense>();

        var profiles = await _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => ids.Contains(p.PersonId))
            .Select(p => new { p.Id, p.PersonId })
            .ToListAsync(ct);

        if (profiles.Count == 0)
            return new Dictionary<Guid, DriverLicense>();

        var profileIds = profiles.Select(p => p.Id).ToList();
        var licenses = await _context.DriverLicenses
            .AsNoTracking()
            .Where(l => profileIds.Contains(l.ProfileId))
            .ToListAsync(ct);

        var personByProfile = profiles.ToDictionary(p => p.Id, p => p.PersonId);
        var result = new Dictionary<Guid, DriverLicense>();

        foreach (var license in licenses)
        {
            if (!personByProfile.TryGetValue(license.ProfileId, out var personId))
                continue;

            result[personId] = new DriverLicense
            {
                Id = license.Id,
                ProfileId = license.ProfileId,
                LicenseNumber = license.LicenseNumber,
                LicenseExpirationDate = license.LicenseExpirationDate,
                Status = license.Status
            };
        }

        return result;
    }
}
