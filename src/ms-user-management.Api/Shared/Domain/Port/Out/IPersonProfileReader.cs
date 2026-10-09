using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Domain.Port.Out;

public interface IPersonProfileReader
{
    Task<IReadOnlyDictionary<Guid, Guid>> GetProfileIdsByPersonIdsAsync(
        IReadOnlyCollection<Guid> personIds,
        CancellationToken ct = default);

    /// <summary>
    /// PersonIds cuyo perfil tiene el rol indicado. Es el filtro que separa
    /// acudientes, estudiantes, conductores y administradores en los listados.
    /// </summary>
    Task<IReadOnlySet<Guid>> GetPersonIdsByRoleAsync(
        RoleId roleId,
        CancellationToken ct = default);

    Task<bool> ProfileExistsAsync(Guid profileId, CancellationToken ct = default);

    Task<string?> GetPersonNameAsync(Guid profileId, CancellationToken ct = default);

    Task<Guid?> GetSchoolIdByCampusIdAsync(Guid campusId, CancellationToken ct = default);
}
