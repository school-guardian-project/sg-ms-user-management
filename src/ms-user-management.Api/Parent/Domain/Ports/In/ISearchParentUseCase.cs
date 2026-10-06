using ms_user_management.Api.Shared.Application.Dto;

namespace ms_user_management.Api.Parent.Domain.Ports.In;

public interface ISearchParentUseCase
{
    Task<IEnumerable<PersonListDto>> ExecuteAsync(string search);
}
