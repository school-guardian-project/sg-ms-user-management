using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Port.Out;
using RoleIds = ms_user_management.Api.Shared.Domain.Model.RoleId;

namespace ms_user_management.Tests.Fakes;

/// <summary>
/// Tenant configurable para pruebas; por defecto simula "sin usuario" (llamada
/// interna, no filtra). Con un rol asignado se considera autenticado, igual que
/// JwtTenantProvider: se acota siempre salvo al SuperAdmin.
/// </summary>
public class FakeTenantProvider : ITenantProvider
{
    public RoleIds? RoleId { get; set; }
    public Guid? CampusId { get; set; }
    public Guid? SchoolId { get; set; }

    /// <summary>Fuerza "autenticado" sin rol (token sin roleId).</summary>
    public bool? Authenticated { get; set; }

    public bool IsSuperAdmin => RoleId == RoleIds.SuperAdmin;

    public bool IsAuthenticated => Authenticated ?? RoleId is not null;

    public bool ShouldFilter => IsAuthenticated && !IsSuperAdmin;
}

public class InMemorySchoolCampusReader : ISchoolCampusReader
{
    public readonly Dictionary<Guid, List<Guid>> CampusIdsBySchool = new();

    public Task<IReadOnlyCollection<Guid>> GetCampusIdsBySchoolAsync(Guid schoolId, CancellationToken ct = default)
    {
        IReadOnlyCollection<Guid> result =
            CampusIdsBySchool.TryGetValue(schoolId, out var campusIds) ? campusIds : [];

        return Task.FromResult(result);
    }
}

public static class TenantFakes
{
    /// <summary>Filtro que no acota por tenant (equivalente a llamada interna sin token).</summary>
    public static TenantPersonFilter NoFilter(IPersonProfileReader profiles) =>
        new(new FakeTenantProvider(), profiles, new InMemorySchoolCampusReader());
}
