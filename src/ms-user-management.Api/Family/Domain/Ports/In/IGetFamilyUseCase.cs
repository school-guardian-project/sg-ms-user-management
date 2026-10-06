using ms_user_management.Api.Family.Application.Dto;

namespace ms_user_management.Api.Family.Domain.Ports.In;

public interface IGetFamilyUseCase
{
    Task<FamilyResponseDto> ExecuteAsync(Guid familyId, CancellationToken ct = default);
}
