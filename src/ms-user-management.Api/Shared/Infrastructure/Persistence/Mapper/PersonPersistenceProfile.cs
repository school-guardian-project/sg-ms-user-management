using AutoMapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Mapper;

public class PersonPersistenceProfile : Profile
{
    public PersonPersistenceProfile()
    {
        CreateMap<Person, PersonEntity>();
        CreateMap<PersonEntity, Person>();
    }
}