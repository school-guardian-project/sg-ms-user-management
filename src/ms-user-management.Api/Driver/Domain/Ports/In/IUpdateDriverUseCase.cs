using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Driver.Domain.Ports.In;

public interface IUpdateDriverUseCase
{
    Task UpdateAsync(Guid id, PersonRequestDto dto);
}
