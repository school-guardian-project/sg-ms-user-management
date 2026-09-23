using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Domain.Port.Out;

public interface IPersonSearchRepository
{
    Task<IEnumerable<Person>> SearchByEmailAsync(string email);

    Task<IEnumerable<Person>> SearchByIdentificationAsync(string identificationNumber);

    Task<IEnumerable<Person>> SearchByNameAsync(string search);
}