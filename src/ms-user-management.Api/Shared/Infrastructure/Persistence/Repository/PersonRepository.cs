using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;
using Status = ms_user_management.Api.Shared.Domain.Model.Status;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

public class PersonRepository : IPersonRepository
{
    private readonly UserManagementContext _context;
    private readonly IMapper _mapper;

    public PersonRepository(UserManagementContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task SaveAsync(Person person)
    {
        var entity = _mapper.Map<PersonEntity>(person);

        await _context.Person.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<Person> GetByIdAsync(Guid id)
    {
        var entity = await _context.Person.FindAsync(id);

        return entity == null ? null : _mapper.Map<Person>(entity);
    }

    public async Task<IEnumerable<Person>> GetAllAsync()
    {
        var entity = await _context.Person
            .Where(p => p.Status == Status.Active)
            .ToListAsync();
        return _mapper.Map<IEnumerable<Person>>(entity);
    }

    public async Task UpdateAsync(Person person)
    {
        var entity = await _context.Person.FirstOrDefaultAsync(x => x.Id == person.Id);

        if (entity == null) throw new Exception("Person not found");

        _mapper.Map(person, entity);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _context.Person.FindAsync(id);

        if (entity == null) throw new Exception("Person not found");

        // Borrado lógico: Iam.Profile y otras ~13 tablas referencian a la persona sin cascada.
        await using var tx = await _context.Database.BeginTransactionAsync();

        entity.Status = Status.Inactive;
        await _context.SaveChangesAsync();

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Iam.Profile SET Status = 'Inactive' WHERE PersonId = {id}");
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE fm FROM UserManagement.FamilyMember fm JOIN Iam.Profile p ON p.Id = fm.ProfileId WHERE p.PersonId = {id}");

        await tx.CommitAsync();
    }
}