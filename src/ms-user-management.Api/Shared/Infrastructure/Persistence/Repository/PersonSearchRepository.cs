using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Context;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;

public class PersonSearchRepository : IPersonSearchRepository
{
    private readonly UserManagementContext _context;
    private readonly IMapper _mapper;

    public PersonSearchRepository(UserManagementContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<Person>> SearchByEmailAsync(string email)
    {
        var entities = await _context.Person
            .AsNoTracking()
            .Where(p => p.Email == email)
            .ToListAsync();

        return _mapper.Map<IEnumerable<Person>>(entities);
    }

    public async Task<IEnumerable<Person>> SearchByIdentificationAsync(string identificationNumber)
    {
        var entities = await _context.Person
            .AsNoTracking()
            .Where(p => p.IdentificationNumber == identificationNumber)
            .ToListAsync();

        return _mapper.Map<IEnumerable<Person>>(entities);
    }

    public async Task<IEnumerable<Person>> SearchByNameAsync(string name)
    {
        var entities = await _context.Person
            .AsNoTracking()
            .Where(p => p.Name == name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<Person>>(entities);
    }
}