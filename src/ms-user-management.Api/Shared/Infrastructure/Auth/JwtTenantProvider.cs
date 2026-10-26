using System.Security.Claims;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Infrastructure.Auth;

/// <summary>
/// Lee el tenant desde HttpContext.User (claims roleId/campusId/schoolId emitidos por ms-iam).
/// Sin token o token inválido => no se filtra (compatibilidad con llamadas internas).
/// </summary>
public class JwtTenantProvider : ITenantProvider
{
    private readonly ClaimsPrincipal? _user;

    public JwtTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    RoleId? ITenantProvider.RoleId => ParseRole(GetClaim("roleId"));

    public Guid? CampusId => ParseGuid(GetClaim("campusId"));

    public Guid? SchoolId => ParseGuid(GetClaim("schoolId"));

    public bool IsSuperAdmin => ((ITenantProvider)this).RoleId == RoleId.SuperAdmin;

    public bool ShouldFilter
    {
        get
        {
            var role = ((ITenantProvider)this).RoleId;
            return role.HasValue && role.Value != RoleId.SuperAdmin;
        }
    }

    private string? GetClaim(string type) =>
        _user is { Identity.IsAuthenticated: true }
            ? _user.FindFirst(type)?.Value
            : null;

    private static RoleId? ParseRole(string? value) =>
        Enum.TryParse<RoleId>(value, out var result) ? result : null;

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var result) ? result : null;
}
