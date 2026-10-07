using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Tests.Fakes;

public class InMemoryPersonLicenseReader : IPersonLicenseReader
{
    public readonly Dictionary<Guid, DriverLicense> LicensesByPerson = new();

    public Task<IReadOnlyDictionary<Guid, DriverLicense>> GetByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, DriverLicense> result = LicensesByPerson
            .Where(kv => personIds.Contains(kv.Key))
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return Task.FromResult(result);
    }
}
