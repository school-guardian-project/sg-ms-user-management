using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Admin.Domain.Ports.In;

public interface IUpdateAdminUseCase
{
    Task UpdateAsync(Guid id, PersonRequestDto dto);
}
