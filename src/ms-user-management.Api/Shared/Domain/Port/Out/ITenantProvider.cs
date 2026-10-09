using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Domain.Port.Out;

/// <summary>
/// Expone el tenant (colegio/sede) del usuario autenticado según los claims del JWT.
/// </summary>
public interface ITenantProvider
{
    RoleId? RoleId { get; }
    Guid? CampusId { get; }
    Guid? SchoolId { get; }
    bool IsSuperAdmin { get; }

    /// <summary>
    /// true cuando el token es válido y el rol exige acotar los listados por tenant.
    /// Sin token, token inválido o SuperAdmin (roleId 5) => false.
    /// </summary>
    bool ShouldFilter { get; }
}
