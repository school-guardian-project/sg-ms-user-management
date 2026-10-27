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
    /// true cuando hay un usuario con access token valido. false en las llamadas
    /// internas sin usuario (gRPC).
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// true cuando hay usuario autenticado y no es SuperAdmin: hay que acotar por
    /// tenant. Un usuario autenticado sin rol reconocible tambien se acota (y como
    /// no tiene tenant resoluble, no ve nada). Sin usuario => false.
    /// </summary>
    bool ShouldFilter { get; }
}
