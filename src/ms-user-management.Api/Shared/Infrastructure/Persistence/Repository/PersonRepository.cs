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
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();

            var affected = await _context.Person
                .Where(p => p.Id == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Status, Status.Inactive));
            if (affected == 0) throw new Exception("Person not found");

            // Set-based writes are replayed after rollback without stale tracked entities.
            await _context.ProfileRefEntities
                .Where(p => p.PersonId == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Status, "Inactive"));

            await _context.FamilyMembers
                .Where(member => _context.ProfileRefEntities
                    .Any(profile => profile.Id == member.ProfileId && profile.PersonId == id))
                .ExecuteDeleteAsync();

            await tx.CommitAsync();
        });
    }
}