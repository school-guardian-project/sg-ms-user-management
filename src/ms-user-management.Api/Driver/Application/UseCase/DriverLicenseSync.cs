using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Driver.Application.UseCase;

/// <summary>
/// Persiste la licencia que viaja en el formulario del conductor. Es opcional:
/// si no llega ni numero ni fecha no se toca nada.
/// </summary>
public static class DriverLicenseSync
{
    public static bool HasLicense(PersonRequestDto dto) =>
        !string.IsNullOrWhiteSpace(dto.LicenseNumber) || dto.LicenseExpirationDate is not null;

    public static async Task<bool> SaveAsync(
        IPersonLicenseWriter writer,
        Guid personId,
        PersonRequestDto dto,
        CancellationToken ct = default)
    {
        var number = DriverLicenseRules.ValidateLicenseNumber(dto.LicenseNumber ?? string.Empty);
        var expiration = DriverLicenseRules.ValidateExpirationDate(dto.LicenseExpirationDate ?? default);

        return await writer.UpsertByPersonIdAsync(personId, number, expiration, ct);
    }
}
