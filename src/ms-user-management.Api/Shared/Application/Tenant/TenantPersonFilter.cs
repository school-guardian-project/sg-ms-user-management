using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Application.Tenant;

/// <summary>
/// Acota los listados al tenant del usuario autenticado:
/// admin (roleId 1) => sedes de su schoolId (School.SchoolCampus);
/// student/driver/parent (roleId 2/3/4) => su campusId.
/// Sin token, token inválido o SuperAdmin => no se filtra (devuelve todos los del rol).
/// </summary>
public class TenantPersonFilter
{
    private readonly ITenantProvider _tenant;
    private readonly IPersonProfileReader _profileReader;
    private readonly ISchoolCampusReader _schoolCampusReader;

    public TenantPersonFilter(
        ITenantProvider tenant,
        IPersonProfileReader profileReader,
        ISchoolCampusReader schoolCampusReader)
    {
        _tenant = tenant;
        _profileReader = profileReader;
        _schoolCampusReader = schoolCampusReader;
    }

    /// <summary>
    /// Sedes visibles para el token actual, o null cuando no aplica filtro de tenant.
    /// </summary>
    public async Task<IReadOnlyCollection<Guid>?> GetVisibleCampusIdsAsync(CancellationToken ct = default)
    {
        if (!_tenant.ShouldFilter)
            return null;

        if (_tenant.RoleId == RoleId.Admin)
        {
            return _tenant.SchoolId is { } schoolId
                ? await _schoolCampusReader.GetCampusIdsBySchoolAsync(schoolId, ct)
                : null;
        }

        return _tenant.CampusId is { } campusId ? new[] { campusId } : null;
    }

    /// <summary>
    /// PersonIds con el rol indicado visibles para el token actual: los del rol
    /// acotados a las sedes del tenant, o todos los del rol si no hay filtro.
    /// </summary>
    public async Task<IReadOnlySet<Guid>> GetVisiblePersonIdsAsync(RoleId roleId, CancellationToken ct = default)
    {
        var campusIds = await GetVisibleCampusIdsAsync(ct);

        return await _profileReader.GetPersonIdsByRoleAsync(roleId, campusIds, ct);
    }
}
