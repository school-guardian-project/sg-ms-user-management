using ms_user_management.Api.Family.Application.Dto;

namespace ms_user_management.Api.Family.Domain.Ports.In;

public interface IListFamilyUseCase
{
    Task<IEnumerable<FamilyListDto>> ExecuteAsync(CancellationToken ct = default);
}
