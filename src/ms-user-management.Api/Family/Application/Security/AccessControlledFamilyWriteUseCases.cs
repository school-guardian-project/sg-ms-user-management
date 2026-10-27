using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Exceptions;

namespace ms_user_management.Api.Family.Application.Security;

/// <summary>
/// Edicion de una familia: debe ser del colegio del staff (misma regla que el
/// detalle) y todos sus miembros nuevos tambien; si no, un admin podria meter
/// estudiantes de otro colegio en una familia suya.
/// </summary>
public sealed class AccessControlledUpdateFamilyUseCase : IUpdateFamilyUseCase
{
    private readonly IUpdateFamilyUseCase _inner;
    private readonly IListFamilyUseCase _tenantFamilies;
    private readonly PersonAccessGuard _guard;

    /// <param name="tenantFamilies">Listado acotado por tenant, sin decorar.</param>
    public AccessControlledUpdateFamilyUseCase(
        IUpdateFamilyUseCase inner, IListFamilyUseCase tenantFamilies, PersonAccessGuard guard)
    {
        _inner = inner;
        _tenantFamilies = tenantFamilies;
        _guard = guard;
    }

    public async Task ExecuteAsync(Guid familyId,
        string familyName,
        string? observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default)
    {
        await _guard.EnsureProfilesInTenantAsync(members.Select(m => m.ProfileId), ct);
        await TenantFamilies.EnsureContainsAsync(_tenantFamilies, familyId, ct);

        await _inner.ExecuteAsync(familyId, familyName, observations, members, ct);
    }
}

/// <summary>Baja de una familia solo dentro del colegio del staff.</summary>
public sealed class AccessControlledDeleteFamilyUseCase : IDeleteFamilyUseCase
{
    private readonly IDeleteFamilyUseCase _inner;
    private readonly IListFamilyUseCase _tenantFamilies;
    private readonly PersonAccessGuard _guard;

    /// <param name="tenantFamilies">Listado acotado por tenant, sin decorar.</param>
    public AccessControlledDeleteFamilyUseCase(
        IDeleteFamilyUseCase inner, IListFamilyUseCase tenantFamilies, PersonAccessGuard guard)
    {
        _inner = inner;
        _tenantFamilies = tenantFamilies;
        _guard = guard;
    }

    public async Task ExecuteAsync(Guid familyId, CancellationToken ct = default)
    {
        _guard.EnsureAuthenticated();
        await TenantFamilies.EnsureContainsAsync(_tenantFamilies, familyId, ct);

        await _inner.ExecuteAsync(familyId, ct);
    }
}

internal static class TenantFamilies
{
    /// <summary>
    /// La familia debe estar en el listado acotado por tenant (para el
    /// SuperAdmin ese listado no se acota).
    /// </summary>
    public static async Task EnsureContainsAsync(IListFamilyUseCase tenantFamilies, Guid familyId, CancellationToken ct)
    {
        if (!(await tenantFamilies.ExecuteAsync(ct)).Any(f => f.Id == familyId))
            throw new AccessDeniedException("The family is outside your school.");
    }
}
