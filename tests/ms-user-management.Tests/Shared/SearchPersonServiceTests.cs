using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Application.Search;
using ms_user_management.Api.Shared.Application.Search.Strategy;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Api.Shared.Infrastructure.Persistence.Repository;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SearchPersonServiceTests
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

    [Test]
    public async Task Search_TerminoVacio_RetornaVacio()
    {
        var service = new SearchPersonService(
            new IPersonSearchStrategy[] { new NameSearchStrategy(new FakePersonSearchRepository()) },
            new InMemoryPersonLicenseReader(),
            new InMemoryPersonProfileReader(),
            TenantFakes.NoFilter(new InMemoryPersonProfileReader()),
            CreateMapper());

        var result = await service.SearchAsync("   ");

        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task Search_ConLicencia_RellenaLicenciaYVencimiento()
    {
        var personId = Guid.NewGuid();
        var repo = new FakePersonSearchRepository();
        repo.People.Add(Driver(personId, "Ana", "ana@mail.com"));

        var reader = new InMemoryPersonLicenseReader();
        reader.LicensesByPerson[personId] = new DriverLicense
        {
            ProfileId = Guid.NewGuid(),
            LicenseNumber = "ABC12345",
            LicenseExpirationDate = new DateOnly(2030, 12, 31),
            Status = default
        };

        var service = new SearchPersonService(
            new IPersonSearchStrategy[] { new NameSearchStrategy(repo) },
            reader,
            new InMemoryPersonProfileReader(),
            TenantFakes.NoFilter(new InMemoryPersonProfileReader()),
            CreateMapper());

        var result = (await service.SearchAsync("ana")).ToList();

        var singleItems = result;
        Assert.That(singleItems, Has.Exactly(1).Items);
        var single = singleItems.Single();
        Assert.That(single.LicenseNumber, Is.EqualTo("ABC12345"));
        Assert.That(single.LicenseExpirationDate, Is.EqualTo(new DateOnly(2030, 12, 31)));
        Assert.That(single.Email, Is.EqualTo("ana@mail.com"));
    }

    [Test]
    public async Task Search_RellenaProfileId()
    {
        var personId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var repo = new FakePersonSearchRepository();
        repo.People.Add(Driver(personId, "Ana", "ana@mail.com"));

        var profileReader = new InMemoryPersonProfileReader();
        profileReader.ProfileByPerson[personId] = profileId;

        var service = new SearchPersonService(
            new IPersonSearchStrategy[] { new NameSearchStrategy(repo) },
            new InMemoryPersonLicenseReader(),
            profileReader,
            TenantFakes.NoFilter(profileReader),
            CreateMapper());

        var result = (await service.SearchAsync("ana")).ToList();

        var singleItems = result;
        Assert.That(singleItems, Has.Exactly(1).Items);
        var single = singleItems.Single();
        Assert.That(single.ProfileId, Is.EqualTo(profileId));
    }

    [Test]
    public async Task Search_ConRol_NoDevuelvePersonasDeOtroRol()
    {
        var student = Guid.NewGuid();
        var parent = Guid.NewGuid();
        var repo = new FakePersonSearchRepository();
        repo.People.Add(Driver(student, "Ana", "ana@mail.com"));
        repo.People.Add(Driver(parent, "Ana", "otra@mail.com"));

        var profileReader = new InMemoryPersonProfileReader();
        profileReader.RoleByPerson[student] = RoleId.Student;
        profileReader.RoleByPerson[parent] = RoleId.Parent;

        var service = new SearchPersonService(
            new IPersonSearchStrategy[] { new NameSearchStrategy(repo) },
            new InMemoryPersonLicenseReader(),
            profileReader,
            TenantFakes.NoFilter(profileReader),
            CreateMapper());

        // Sin rol "ana" devuelve a las dos; con rol solo la del rol pedido.
        var result = (await service.SearchAsync("ana", RoleId.Student)).ToList();

        Assert.That(result.Single().Id, Is.EqualTo(student));
    }

    private class FakePersonSearchRepository : IPersonSearchRepository
    {
        public List<Person> People { get; } = new();

        public Task<IEnumerable<Person>> SearchByEmailAsync(string email)
            => Task.FromResult<IEnumerable<Person>>(People.Where(p =>
                p.Email.Contains(email, StringComparison.OrdinalIgnoreCase)));

        public Task<IEnumerable<Person>> SearchByIdentificationAsync(string identificationNumber)
            => Task.FromResult<IEnumerable<Person>>(People.Where(p =>
                p.IdentificationNumber.Contains(identificationNumber)));

        public Task<IEnumerable<Person>> SearchByNameAsync(string search)
            => Task.FromResult<IEnumerable<Person>>(People.Where(p =>
                p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || p.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)));
    }
}
