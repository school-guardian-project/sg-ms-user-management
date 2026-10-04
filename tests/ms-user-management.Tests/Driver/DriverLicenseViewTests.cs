using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Tests.Fakes;
using Xunit;

namespace ms_user_management.Tests.Driver;

public class DriverLicenseViewTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PersonProfile>());

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static Person Driver(Guid id, string name, string email) => new()
    {
        Id = id,
        Name = name,
        LastName = "Perez",
        IdentificationType = default,
        IdentificationNumber = "100000" + name.Length,
        Email = email,
        Phone = 300111222,
        ResidenceAddress = "Calle 1",
        DateBirth = new DateOnly(1990, 1, 1),
        Status = default
    };

    private static DriverLicense LicenseFor(Guid personId) => new()
    {
        ProfileId = Guid.NewGuid(), // el lector ya resolvió PersonId -> ProfileId
        LicenseNumber = "ABC12345",
        LicenseExpirationDate = new DateOnly(2030, 12, 31),
        Status = default
    };

    [Fact]
    public async Task List_ConLicencia_RellenaLicenciaYEmail()
    {
        var withLicense = Guid.NewGuid();
        var withoutLicense = Guid.NewGuid();
        var personRepo = new InMemoryPersonRepository(
            Driver(withLicense, "Ana", "ana@mail.com"),
            Driver(withoutLicense, "Luis", "luis@mail.com"));

        var reader = new InMemoryPersonLicenseReader();
        reader.LicensesByPerson[withLicense] = LicenseFor(withLicense);

        var service = new ListDriverService(personRepo, reader, new InMemoryPersonProfileReader(), CreateMapper());

        var result = (await service.ExecuteAsync()).ToList();

        Assert.Equal(2, result.Count);
        var ana = result.Single(r => r.Id == withLicense);
        Assert.Equal("ABC12345", ana.LicenseNumber);
        Assert.Equal(new DateOnly(2030, 12, 31), ana.LicenseExpirationDate);
        Assert.Equal("ana@mail.com", ana.Email);

        var luis = result.Single(r => r.Id == withoutLicense);
        Assert.Null(luis.LicenseNumber);
        Assert.Null(luis.LicenseExpirationDate);
    }

    [Fact]
    public async Task Get_ConLicencia_RellenaLicenciaEnDetalle()
    {
        var personId = Guid.NewGuid();
        var personRepo = new InMemoryPersonRepository(Driver(personId, "Ana", "ana@mail.com"));

        var reader = new InMemoryPersonLicenseReader();
        reader.LicensesByPerson[personId] = LicenseFor(personId);

        var service = new GetDriverService(personRepo, reader, CreateMapper());

        var detail = await service.GetByIdAsync(personId);

        Assert.Equal("ABC12345", detail.LicenseNumber);
        Assert.Equal(new DateOnly(2030, 12, 31), detail.LicenseExpirationDate);
        Assert.Equal("ana@mail.com", detail.Email);
    }

    private class InMemoryPersonRepository : IPersonRepository
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
}
