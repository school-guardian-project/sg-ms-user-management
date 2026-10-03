using ms_user_management.Api.Driver.Domain.Model;

namespace ms_user_management.Api.Driver.Domain.Ports.Out;

public interface IPersonLicenseReader
{
    Task<IReadOnlyDictionary<Guid, DriverLicense>> GetByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default);
}
