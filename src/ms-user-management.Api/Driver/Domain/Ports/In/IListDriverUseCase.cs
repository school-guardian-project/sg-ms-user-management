using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Driver.Domain.Ports.In;

public interface IListDriverUseCase
{
    Task<IEnumerable<PersonListDto>> ExecuteAsync();
}
