using ms_user_management.Api.Family.Domain.Model;

namespace ms_user_management.Api.Family.Domain.Ports.In;

public interface IUpdateFamilyUseCase
{
    Task ExecuteAsync(Guid familyId,
        string familyName,
        string? observations,
        IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members,
        CancellationToken ct = default);
}
