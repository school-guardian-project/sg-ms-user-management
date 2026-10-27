using System.Security.Claims;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Infrastructure.Auth;

/// <summary>
/// Lee el tenant desde HttpContext.User (claims roleId/campusId/schoolId emitidos por ms-iam).
/// Sin usuario (llamadas internas gRPC) no se filtra; un usuario autenticado
/// siempre se acota salvo el SuperAdmin, tambien si su rol falta o no se reconoce.
/// </summary>
public class JwtTenantProvider : ITenantProvider
{
    private readonly ClaimsPrincipal? _user;

    public JwtTenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    RoleId? ITenantProvider.RoleId => ParseRole(GetClaim(SgClaimTypes.RoleId));

    public Guid? CampusId => ParseGuid(GetClaim(SgClaimTypes.CampusId));

    public Guid? SchoolId => ParseGuid(GetClaim(SgClaimTypes.SchoolId));

    public bool IsSuperAdmin => ((ITenantProvider)this).RoleId == RoleId.SuperAdmin;

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    public bool ShouldFilter => IsAuthenticated && !IsSuperAdmin;

    private string? GetClaim(string type) =>
        IsAuthenticated ? _user!.FindFirst(type)?.Value : null;

    // Un roleId fuera del catalogo se trata como ausente (fail-closed).
    private static RoleId? ParseRole(string? value) =>
        Enum.TryParse<RoleId>(value, out var result) && Enum.IsDefined(result) ? result : null;

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var result) ? result : null;
}
