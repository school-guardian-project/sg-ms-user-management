using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Parent.Domain.Ports.In;

public interface IUpdateParentUseCase
{
    Task UpdateAsync(Guid id, PersonRequestDto dto);
}
