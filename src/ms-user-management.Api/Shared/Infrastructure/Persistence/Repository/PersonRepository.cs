using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;
using ms_user_management.Api.Family.Infrastructure.Persistence;
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

        // Borrado lógico: Iam.Profile y FamilyMember referencian a la persona sin cascada.
        // Todo va en la misma transacción con EF Core (sin SQL crudo) para no dejar
        // la BD a medio borrar si alguna operación falla.
        await using var tx = await _context.Database.BeginTransactionAsync();

        try
        {
            entity.Status = Status.Inactive;
            await _context.SaveChangesAsync();

            // Desactivar perfiles de Iam asociados a la persona
            await _context.ProfileRefEntities
                .Where(p => p.PersonId == id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Status, "Inactive"));

            // Borrar familiares cuyo perfil pertenece a la persona
            var memberIds = await _context.FamilyMembers
                .Join(_context.ProfileRefEntities,
                    fm => fm.ProfileId,
                    p => p.Id,
                    (fm, p) => new { fm.Id, p.PersonId })
                .Where(x => x.PersonId == id)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var memberId in memberIds)
            {
                var member = new FamilyMemberEntity { Id = memberId };
                _context.FamilyMembers.Attach(member);
                _context.FamilyMembers.Remove(member);
            }
            await _context.SaveChangesAsync();

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}