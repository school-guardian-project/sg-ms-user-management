using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Driver.Infrastructure.Persistence;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;

namespace ms_user_management.Api.Driver.Infrastructure.Repository;

public class DriverLicenseRepositoryImpl : IDriverLicenseRepository
{
    private readonly UserManagementContext _context;

    public DriverLicenseRepositoryImpl(UserManagementContext context)
    {
        _context = context;
    }

    public async Task<Guid> SaveAsync(DriverLicense license, CancellationToken ct = default)
    {
        _context.DriverLicenses.Add(ToEntity(license));
        await _context.SaveChangesAsync(ct);

        return license.Id;
    }

    public async Task<IReadOnlyList<DriverLicense>> GetAllAsync(Guid? profileId = null, CancellationToken ct = default)
    {
        var query = _context.DriverLicenses.AsNoTracking();

        if (profileId is not null)
            query = query.Where(x => x.ProfileId == profileId);

        var rows = await query.ToListAsync(ct);

        return rows.Select(ToDomain).ToList();
    }

    public async Task<DriverLicense?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var row = await _context.DriverLicenses.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return row is null ? null : ToDomain(row);
    }

    public async Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeId = null, CancellationToken ct = default)
    {
        return await _context.DriverLicenses.AsNoTracking()
            .AnyAsync(x => x.LicenseNumber == licenseNumber && (excludeId == null || x.Id != excludeId), ct);
    }

    public async Task<bool> UpdateAsync(DriverLicense license, CancellationToken ct = default)
    {
        var entity = await _context.DriverLicenses.FirstOrDefaultAsync(x => x.Id == license.Id, ct);

        if (entity is null) return false;

        entity.LicenseNumber = license.LicenseNumber;
        entity.LicenseExpirationDate = license.LicenseExpirationDate;
        entity.Status = license.Status;

        await _context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _context.DriverLicenses.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (entity is null) return false;

        _context.DriverLicenses.Remove(entity);
        await _context.SaveChangesAsync(ct);

        return true;
    }

    private static DriverLicenseEntity ToEntity(DriverLicense license) => new()
    {
        Id = license.Id,
        ProfileId = license.ProfileId,
        LicenseNumber = license.LicenseNumber,
        LicenseExpirationDate = license.LicenseExpirationDate,
        Status = license.Status
    };

    private static DriverLicense ToDomain(DriverLicenseEntity entity) => new()
    {
        Id = entity.Id,
        ProfileId = entity.ProfileId,
        LicenseNumber = entity.LicenseNumber,
        LicenseExpirationDate = entity.LicenseExpirationDate,
        Status = entity.Status
    };
}
