using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class UpdateDriverLicenseService
{
    private readonly IDriverLicenseRepository _repository;

    public UpdateDriverLicenseService(IDriverLicenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> ExecuteAsync(Guid id, UpdateDriverLicenseRequestDto dto, CancellationToken ct = default)
    {
        var current = await _repository.GetByIdAsync(id, ct);
        if (current is null) return false;

        var licenseNumber = DriverLicenseRules.ValidateLicenseNumber(dto.LicenseNumber);
        var expirationDate = DriverLicenseRules.ValidateExpirationDate(dto.LicenseExpirationDate);

        if (await _repository.LicenseNumberExistsAsync(licenseNumber, id, ct))
            throw new InvalidOperationException($"License number already exists: {licenseNumber}");

        var license = new DriverLicense
        {
            Id = current.Id,
            ProfileId = current.ProfileId,
            LicenseNumber = licenseNumber,
            LicenseExpirationDate = expirationDate,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? current.Status : DriverLicenseRules.ParseStatus(dto.Status)
        };

        return await _repository.UpdateAsync(license, ct);
    }
}
