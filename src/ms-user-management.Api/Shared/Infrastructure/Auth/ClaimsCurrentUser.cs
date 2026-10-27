using System.Security.Claims;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Infrastructure.Auth;

/// <summary>
/// Usuario actual leido de HttpContext.User, que solo se puebla con un access
/// token validado (ver <see cref="JwtAuthenticationExtensions"/>).
/// </summary>
public sealed class ClaimsCurrentUser : ICurrentUser
{
    private readonly ClaimsPrincipal? _user;

    public ClaimsCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _user = httpContextAccessor.HttpContext?.User;
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated == true;

    public Guid? ProfileId => ParseGuid(GetClaim(SgClaimTypes.Subject));

    public Guid? PersonId => ParseGuid(GetClaim(SgClaimTypes.PersonId));

    public RoleId? RoleId =>
        Enum.TryParse<RoleId>(GetClaim(SgClaimTypes.RoleId), out var role) && Enum.IsDefined(role)
            ? role
            : null;

    public bool HasPermission(string permission) =>
        IsAuthenticated && _user!.HasClaim(SgClaimTypes.Permissions, permission);

    private string? GetClaim(string type) => IsAuthenticated ? _user!.FindFirst(type)?.Value : null;

    private static Guid? ParseGuid(string? value) =>
        Guid.TryParse(value, out var result) && result != Guid.Empty ? result : null;
}
