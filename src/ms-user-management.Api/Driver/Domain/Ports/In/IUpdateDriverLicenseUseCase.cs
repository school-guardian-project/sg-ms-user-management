using ms_user_management.Api.Driver.Application.Dto;

namespace ms_user_management.Api.Driver.Domain.Ports.In;

public interface IUpdateDriverLicenseUseCase
{
    /// <returns>false si la licencia no existe.</returns>
    Task<bool> ExecuteAsync(Guid id, UpdateDriverLicenseRequestDto dto, CancellationToken ct = default);
}
