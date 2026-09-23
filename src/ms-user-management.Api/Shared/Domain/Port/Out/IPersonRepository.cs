using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Domain.Port.Out;

public interface IPersonRepository
{
    Task SaveAsync(Person person);

    Task<Person> GetByIdAsync(Guid id);

    Task<IEnumerable<Person>> GetAllAsync();

    Task UpdateAsync(Person person);

    Task DeleteAsync(Guid id);
}