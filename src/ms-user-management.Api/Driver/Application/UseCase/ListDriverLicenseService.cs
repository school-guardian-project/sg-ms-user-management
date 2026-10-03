using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class ListDriverLicenseService
{
    private readonly IDriverLicenseRepository _repository;

    public ListDriverLicenseService(IDriverLicenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<DriverLicenseListDto>> ExecuteAsync(
        Guid? profileId = null, CancellationToken ct = default)
    {
        var licenses = await _repository.GetAllAsync(profileId, ct);

        return licenses.Select(l => new DriverLicenseListDto
        {
            Id = l.Id,
            ProfileId = l.ProfileId,
            LicenseNumber = l.LicenseNumber,
            LicenseExpirationDate = l.LicenseExpirationDate
        }).ToList();
    }
}
