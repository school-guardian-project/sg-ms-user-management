using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Tests.Fakes;

/// <summary>
/// Repositorio de personas en memoria que recuerda lo que le pidieron.
///
/// Se mantiene aparte de <see cref="InMemoryPersonRepository"/> a proposito: ese
/// da por sentado que la persona existe (busca con <c>Single</c> y revienta si no
/// esta), asi que no sirve para el caso "no encontrada" que aqui interesa probar.
/// Este devuelve la persona o <c>null</c>, y deja ver que se guardo o se borro.
/// </summary>
public class RecordingPersonRepository : IPersonRepository
{
    private readonly List<Person> _persons;

    public RecordingPersonRepository(params Person[] persons) => _persons = persons.ToList();

    /// <summary>La persona que se mando a <see cref="UpdateAsync"/> (null si no se llamo).</summary>
    public Person? Updated { get; private set; }

    /// <summary>El id que se mando a <see cref="DeleteAsync"/> (null si no se llamo).</summary>
    public Guid? DeletedId { get; private set; }

    public Task SaveAsync(Person person)
    {
        _persons.Add(person);
        return Task.CompletedTask;
    }

    public Task<Person> GetByIdAsync(Guid id) =>
        Task.FromResult(_persons.FirstOrDefault(p => p.Id == id)!);

    public Task<IEnumerable<Person>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Person>>(_persons);

    public Task UpdateAsync(Person person)
    {
        Updated = person;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        DeletedId = id;
        _persons.RemoveAll(p => p.Id == id);
        return Task.CompletedTask;
    }
}
