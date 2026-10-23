using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Driver.Application.Mapper;

/// <summary>
/// Conversiones DTO ↔ dominio de las licencias de conducción, centralizando el
/// mapeo que antes estaba escrito a mano en cada caso de uso. La lógica es
/// idéntica: mismos campos, mismo orden, mismo significado.
/// </summary>
public static class DriverLicenseMapper
{
    public static DriverLicenseListDto ToListDto(DriverLicense license) =>
        new DriverLicenseListDto
        {
            Id = license.Id,
            ProfileId = license.ProfileId,
            LicenseNumber = license.LicenseNumber,
            LicenseExpirationDate = license.LicenseExpirationDate
        };

    public static DriverLicenseResponseDto ToResponseDto(DriverLicense license) =>
        new DriverLicenseResponseDto
        {
            Id = license.Id,
            ProfileId = license.ProfileId,
            LicenseNumber = license.LicenseNumber,
            LicenseExpirationDate = license.LicenseExpirationDate,
            Status = license.Status
        };

    public static DriverLicense ToDomain(
        DriverLicenseRequestDto dto, string licenseNumber, DateOnly expirationDate) =>
        new DriverLicense
        {
            Id = Guid.NewGuid(),
            ProfileId = dto.ProfileId,
            LicenseNumber = licenseNumber,
            LicenseExpirationDate = expirationDate,
            Status = Status.Active
        };

    public static DriverLicense ToUpdatedDomain(
        DriverLicense current, string licenseNumber, DateOnly expirationDate, string? status) =>
        new DriverLicense
        {
            Id = current.Id,
            ProfileId = current.ProfileId,
            LicenseNumber = licenseNumber,
            LicenseExpirationDate = expirationDate,
            Status = string.IsNullOrWhiteSpace(status) ? current.Status : DriverLicenseRules.ParseStatus(status)
        };
}
