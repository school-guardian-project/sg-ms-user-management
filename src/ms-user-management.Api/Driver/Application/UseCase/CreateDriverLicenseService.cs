using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.Mapper;
using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class CreateDriverLicenseService
{
    private readonly IDriverLicenseRepository _repository;

    public CreateDriverLicenseService(IDriverLicenseRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> ExecuteAsync(DriverLicenseRequestDto dto, CancellationToken ct = default)
    {
        if (dto.ProfileId == Guid.Empty)
            throw new ArgumentException("ProfileId is required.", nameof(dto));

        var licenseNumber = DriverLicenseRules.ValidateLicenseNumber(dto.LicenseNumber);
        var expirationDate = DriverLicenseRules.ValidateExpirationDate(dto.LicenseExpirationDate);

        if (await _repository.LicenseNumberExistsAsync(licenseNumber, null, ct))
            throw new InvalidOperationException($"License number already exists: {licenseNumber}");

        var license = DriverLicenseMapper.ToDomain(dto, licenseNumber, expirationDate);

        return await _repository.SaveAsync(license, ct);
    }
}
