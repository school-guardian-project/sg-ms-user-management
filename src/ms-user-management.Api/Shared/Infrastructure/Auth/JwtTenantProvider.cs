using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Infrastructure.Auth;

/// <summary>
/// Lee el tenant desde HttpContext.User (claims roleId/campusId/schoolId emitidos por ms-iam).
/// Sin token o token inválido => no se filtra (compatibilidad con llamadas internas entre servicios).
/// </summary>
public class JwtTenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public JwtTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private System.Security.Claims.ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public RoleId? RoleId =>
        byte.TryParse(GetClaim("roleId"), out var roleId) && Enum.IsDefined(typeof(RoleId), roleId)
            ? (RoleId)roleId
            : null;

    public Guid? CampusId => ParseGuid(GetClaim("campusId"));

    public Guid? SchoolId => ParseGuid(GetClaim("schoolId"));

    public bool IsSuperAdmin => RoleId == Shared.Domain.Model.RoleId.SuperAdmin;

    public bool ShouldFilter => RoleId switch
    {
        null => false,
        Shared.Domain.Model.RoleId.SuperAdmin => false,
        Shared.Domain.Model.RoleId.Admin => SchoolId.HasValue,
        _ => CampusId.HasValue
    };

    private string? GetClaim(string type) =>
        User is { Identity.IsAuthenticated: true } user
            ? user.FindFirst(type)?.Value
            : null;

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var result) ? result : null;
}
