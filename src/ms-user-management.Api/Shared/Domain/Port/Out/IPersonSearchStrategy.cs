using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Domain.Port.Out;

public interface IPersonSearchStrategy
{
    bool CanHandle(string search);

    Task<IEnumerable<Person>> SearchAsync(string search);
}