using ms_user_management.Api.Driver.Domain.Model;

namespace ms_user_management.Api.Driver.Domain.Ports.Out;

public interface IDriverLicenseRepository
{
    Task<Guid> SaveAsync(DriverLicense license, CancellationToken ct = default);

    Task<IReadOnlyList<DriverLicense>> GetAllAsync(Guid? profileId = null, CancellationToken ct = default);

    Task<DriverLicense?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> LicenseNumberExistsAsync(string licenseNumber, Guid? excludeId = null, CancellationToken ct = default);

    Task<bool> UpdateAsync(DriverLicense license, CancellationToken ct = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
