using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Admin.Domain.Ports.In;

public interface IListAdminUseCase
{
    Task<IEnumerable<PersonListDto>> ExecuteAsync();
}
