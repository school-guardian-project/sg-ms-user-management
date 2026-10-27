using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Shared.Application.Security;

public static class PersonListScope
{
    /// <summary>
    /// Deja solo las personas cuyo perfil esta en <paramref name="profileIds"/>;
    /// con null (staff) devuelve el listado tal cual.
    /// </summary>
    public static IEnumerable<PersonListDto> Apply(
        IEnumerable<PersonListDto> persons, IReadOnlySet<Guid>? profileIds) =>
        profileIds is null
            ? persons
            : persons.Where(p => p.ProfileId is { } id && profileIds.Contains(id)).ToList();
}
