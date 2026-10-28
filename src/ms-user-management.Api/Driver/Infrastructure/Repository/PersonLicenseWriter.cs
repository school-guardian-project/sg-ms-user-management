using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Driver.Infrastructure.Persistence;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Driver.Infrastructure.Repository;

/// <summary>
/// Guarda la licencia a partir del PersonId: DriverLicense cuelga de ProfileId,
/// que se resuelve contra Iam.Profile igual que en <see cref="PersonLicenseReader"/>.
/// </summary>
public class PersonLicenseWriter : IPersonLicenseWriter
{
    private readonly UserManagementContext _context;

    public PersonLicenseWriter(UserManagementContext context)
    {
        _context = context;
    }

    public async Task<bool> UpsertByPersonIdAsync(
        Guid personId,
        string licenseNumber,
        DateOnly expirationDate,
        CancellationToken ct = default)
    {
        var profileId = await _context.Set<ProfileRefEntity>()
            .AsNoTracking()
            .Where(p => p.PersonId == personId)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct);

        if (profileId is null) return false;

        var entity = await _context.DriverLicenses
            .FirstOrDefaultAsync(l => l.ProfileId == profileId, ct);

        var duplicated = await _context.DriverLicenses.AsNoTracking()
            .AnyAsync(l => l.LicenseNumber == licenseNumber && (entity == null || l.Id != entity.Id), ct);

        if (duplicated)
            throw new InvalidOperationException($"License number already exists: {licenseNumber}");

        if (entity is null)
        {
            _context.DriverLicenses.Add(new DriverLicenseEntity
            {
                Id = Guid.NewGuid(),
                ProfileId = profileId.Value,
                LicenseNumber = licenseNumber,
                LicenseExpirationDate = expirationDate,
                Status = Status.Active
            });
        }
        else
        {
            entity.LicenseNumber = licenseNumber;
            entity.LicenseExpirationDate = expirationDate;
        }

        await _context.SaveChangesAsync(ct);

        return true;
    }
}
