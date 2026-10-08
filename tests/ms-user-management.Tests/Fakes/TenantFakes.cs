using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Port.Out;
using RoleIds = ms_user_management.Api.Shared.Domain.Model.RoleId;

namespace ms_user_management.Tests.Fakes;

/// <summary>Tenant configurable para pruebas; por defecto simula "sin token" (no filtra).</summary>
public class FakeTenantProvider : ITenantProvider
{
    public RoleIds? RoleId { get; set; }
    public Guid? CampusId { get; set; }
    public Guid? SchoolId { get; set; }

    public bool IsSuperAdmin => RoleId == RoleIds.SuperAdmin;

    public bool ShouldFilter => RoleId switch
    {
        null => false,
        RoleIds.SuperAdmin => false,
        RoleIds.Admin => SchoolId.HasValue,
        _ => CampusId.HasValue
    };
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
