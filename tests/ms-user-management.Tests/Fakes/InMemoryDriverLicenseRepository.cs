using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Tests.Fakes;

public class InMemoryDriverLicenseRepository : IDriverLicenseRepository
{
    public readonly List<DriverLicense> Licenses = new();

    public Task<Guid> SaveAsync(DriverLicense license, CancellationToken ct = default)
    {
        Licenses.Add(license);
        return Task.FromResult(license.Id);
    }

    public Task<IReadOnlyList<DriverLicense>> GetAllAsync(Guid? profileId = null, CancellationToken ct = default)
    {
        IReadOnlyList<DriverLicense> rows = profileId is null
            ? Licenses.ToList()
            : Licenses.Where(l => l.ProfileId == profileId).ToList();

        return Task.FromResult(rows);
    }

    public Task<DriverLicense?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Licenses.FirstOrDefault(l => l.Id == id));

    public Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeId = null, CancellationToken ct = default)
        => Task.FromResult(Licenses.Any(l =>
            l.LicenseNumber == licenseNumber && (excludeId is null || l.Id != excludeId)));

    public Task<bool> UpdateAsync(DriverLicense license, CancellationToken ct = default)
    {
        var current = Licenses.FirstOrDefault(l => l.Id == license.Id);
        if (current is null) return Task.FromResult(false);

        current.LicenseNumber = license.LicenseNumber;
        current.LicenseExpirationDate = license.LicenseExpirationDate;
        current.Status = license.Status;

        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(Licenses.RemoveAll(l => l.Id == id) > 0);
}
