using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Driver.Domain.Ports.In;

public interface IGetPersonUseCase
{
    Task<PersonResponseDto> GetByIdAsync(Guid id);
}
