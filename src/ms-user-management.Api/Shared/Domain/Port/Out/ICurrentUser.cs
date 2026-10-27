using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Domain.Port.Out;

/// <summary>
/// Identidad del usuario que hace la peticion, tomada del access token ya
/// validado. La capa de aplicacion decide con ella que puede ver cada quien.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>Iam.Profile.Id (claim "sub").</summary>
    Guid? ProfileId { get; }

    /// <summary>UserManagement.Person.Id (claim "personId").</summary>
    Guid? PersonId { get; }

    RoleId? RoleId { get; }

    bool HasPermission(string permission);
}
