using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Tests.Fakes;

public class InMemoryPersonRepository : IPersonRepository
{
    private readonly List<Person> _persons;

    public InMemoryPersonRepository(params Person[] persons) => _persons = persons.ToList();

    public Task SaveAsync(Person person)
    {
        _persons.Add(person);
        return Task.CompletedTask;
    }

    public Task<Person> GetByIdAsync(Guid id) =>
        Task.FromResult(_persons.Single(p => p.Id == id));

    public Task<IEnumerable<Person>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Person>>(_persons);

    public Task UpdateAsync(Person person) => Task.CompletedTask;

    public Task DeleteAsync(Guid id) => Task.CompletedTask;
}
