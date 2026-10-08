using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Admin.Domain.Ports.In;

public interface ICreateAdminUseCase
{
    Task CreateAsync(CreateAdminRequestDto dto);
}