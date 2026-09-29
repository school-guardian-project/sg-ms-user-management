using ms_user_management.Api.Family.Domain.Model;

namespace ms_user_management.Api.Family.Domain.Ports.In;

public interface IRegisterFamilyUseCase
{
    Task<(Guid FamilyId, IReadOnlyList<(Guid MemberId, Guid ProfileId)> Members)> ExecuteAsync(
        string familyName,
        string? observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default);
}
