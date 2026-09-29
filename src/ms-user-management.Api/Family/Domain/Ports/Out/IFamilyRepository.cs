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
}
