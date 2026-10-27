using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Domain.Exceptions;

namespace ms_user_management.Api.Family.Application.Security;

/// <summary>
/// Listado de familias: el staff ve las de su colegio (ya acotadas por tenant);
/// un acudiente o estudiante solo las suyas.
/// </summary>
public sealed class AccessControlledListFamilyUseCase : IListFamilyUseCase
{
    private readonly IListFamilyUseCase _inner;
    private readonly PersonAccessGuard _guard;

    public AccessControlledListFamilyUseCase(IListFamilyUseCase inner, PersonAccessGuard guard)
    {
        _inner = inner;
        _guard = guard;
    }

    public async Task<IEnumerable<FamilyListDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var scope = await _guard.GetFamilyScopeAsync(ct);
        var families = await _inner.ExecuteAsync(ct);

        return scope is null ? families : families.Where(f => scope.Contains(f.Id)).ToList();
    }
}

/// <summary>
/// Detalle de una familia: el staff solo si la familia es de su colegio (la
/// misma regla del listado); un acudiente o estudiante solo si es miembro.
/// </summary>
public sealed class AccessControlledGetFamilyUseCase : IGetFamilyUseCase
{
    private readonly IGetFamilyUseCase _inner;
    private readonly IListFamilyUseCase _tenantFamilies;
    private readonly PersonAccessGuard _guard;

    /// <param name="tenantFamilies">Listado acotado por tenant, sin decorar.</param>
    public AccessControlledGetFamilyUseCase(
        IGetFamilyUseCase inner, IListFamilyUseCase tenantFamilies, PersonAccessGuard guard)
    {
        _inner = inner;
        _tenantFamilies = tenantFamilies;
        _guard = guard;
    }

    public async Task<FamilyResponseDto> ExecuteAsync(Guid familyId, CancellationToken ct = default)
    {
        var scope = await _guard.GetFamilyScopeAsync(ct);
        var allowed = scope is null
            ? (await _tenantFamilies.ExecuteAsync(ct)).Any(f => f.Id == familyId)
            : scope.Contains(familyId);

        if (!allowed)
            throw new AccessDeniedException();

        return await _inner.ExecuteAsync(familyId, ct);
    }
}
