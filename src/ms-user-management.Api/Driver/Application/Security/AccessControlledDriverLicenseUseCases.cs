using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Driver.Domain.Ports.Out;
using ms_user_management.Api.Shared.Application.Security;

namespace ms_user_management.Api.Driver.Application.Security;

/// <summary>
/// Edicion de una licencia solo si su conductor es del colegio del staff. Una
/// licencia inexistente sigue al caso de uso, que responde "no encontrada".
/// </summary>
public sealed class AccessControlledUpdateDriverLicenseUseCase : IUpdateDriverLicenseUseCase
{
    private readonly IUpdateDriverLicenseUseCase _inner;
    private readonly IDriverLicenseRepository _licenses;
    private readonly PersonAccessGuard _guard;

    public AccessControlledUpdateDriverLicenseUseCase(
        IUpdateDriverLicenseUseCase inner, IDriverLicenseRepository licenses, PersonAccessGuard guard)
    {
        _inner = inner;
        _licenses = licenses;
        _guard = guard;
    }

    public async Task<bool> ExecuteAsync(Guid id, UpdateDriverLicenseRequestDto dto, CancellationToken ct = default)
    {
        await DriverLicenseOwnership.EnsureInTenantAsync(_licenses, _guard, id, ct);
        return await _inner.ExecuteAsync(id, dto, ct);
    }
}

/// <summary>Baja de una licencia solo si su conductor es del colegio del staff.</summary>
public sealed class AccessControlledDeleteDriverLicenseUseCase : IDeleteDriverLicenseUseCase
{
    private readonly IDeleteDriverLicenseUseCase _inner;
    private readonly IDriverLicenseRepository _licenses;
    private readonly PersonAccessGuard _guard;

    public AccessControlledDeleteDriverLicenseUseCase(
        IDeleteDriverLicenseUseCase inner, IDriverLicenseRepository licenses, PersonAccessGuard guard)
    {
        _inner = inner;
        _licenses = licenses;
        _guard = guard;
    }

    public async Task<bool> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        await DriverLicenseOwnership.EnsureInTenantAsync(_licenses, _guard, id, ct);
        return await _inner.ExecuteAsync(id, ct);
    }
}

internal static class DriverLicenseOwnership
{
    public static async Task EnsureInTenantAsync(
        IDriverLicenseRepository licenses, PersonAccessGuard guard, Guid licenseId, CancellationToken ct)
    {
        guard.EnsureAuthenticated();

        var license = await licenses.GetByIdAsync(licenseId, ct);
        if (license is not null)
            await guard.EnsureProfilesInTenantAsync([license.ProfileId], ct);
    }
}
