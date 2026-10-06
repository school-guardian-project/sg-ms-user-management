using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Admin.Domain.Ports.In;

public interface ISearchAdminUseCase
{
    Task<IEnumerable<PersonListDto>> ExecuteAsync(string search);
}
