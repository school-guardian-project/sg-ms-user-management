namespace ms_user_management.Api.Shared.Domain.Port.Out;

/// <summary>
/// Pertenencia a familias (UserManagement.FamilyMember). Es lo que acota lo que
/// un acudiente o un estudiante puede ver: su familia y los miembros de ella.
/// </summary>
public interface IFamilyMembershipReader
{
    /// <summary>Familias en las que el perfil es miembro (acudiente o estudiante).</summary>
    Task<IReadOnlySet<Guid>> GetFamilyIdsByMemberAsync(Guid profileId, CancellationToken ct = default);

    /// <summary>ProfileIds de todos los miembros de las familias indicadas.</summary>
    Task<IReadOnlySet<Guid>> GetMemberProfileIdsAsync(IReadOnlyCollection<Guid> familyIds, CancellationToken ct = default);
}
