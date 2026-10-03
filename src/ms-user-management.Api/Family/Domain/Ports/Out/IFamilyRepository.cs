using ms_user_management.Api.Family.Domain.Model;
using FamilyModel = ms_user_management.Api.Family.Domain.Model.Family;

namespace ms_user_management.Api.Family.Domain.Ports.Out;

public interface IFamilyRepository
{
    Task<IReadOnlyList<Guid>> SaveAsync(FamilyModel family,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default);

    Task<IReadOnlyList<Guid>> GetAssociatedProfileIdsAsync(
        IEnumerable<Guid> profileIds, CancellationToken ct = default);

    Task<IReadOnlyList<(FamilyModel Family, Guid? ParentProfileId)>> GetAllAsync(
        CancellationToken ct = default);

    Task<IReadOnlyList<FamilySearchRow>> GetAllWithGuardianAsync(
        CancellationToken ct = default);

    Task<FamilyModel?> GetByIdAsync(Guid familyId, CancellationToken ct = default);

    Task<IReadOnlyList<(Guid ProfileId, RelationType RelationType)>> GetMembersAsync(
        Guid familyId, CancellationToken ct = default);

    Task UpdateAsync(Guid familyId, string familyName, string observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default);

    Task DeleteAsync(Guid familyId, CancellationToken ct = default);
}
