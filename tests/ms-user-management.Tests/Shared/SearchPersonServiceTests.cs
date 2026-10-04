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
using Xunit;

namespace ms_user_management.Tests.Shared;

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

    [Fact]
    public async Task Search_TerminoVacio_RetornaVacio()
    {
        var service = new SearchPersonService(
            new IPersonSearchStrategy[] { new NameSearchStrategy(new FakePersonSearchRepository()) },
            new InMemoryPersonLicenseReader(),
            new InMemoryPersonProfileReader(),
            CreateMapper());

        var result = await service.SearchAsync("   ");

        Assert.Empty(result);
    }

    [Fact]
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
            CreateMapper());

        var result = (await service.SearchAsync("ana")).ToList();

        var single = Assert.Single(result);
        Assert.Equal("ABC12345", single.LicenseNumber);
        Assert.Equal(new DateOnly(2030, 12, 31), single.LicenseExpirationDate);
        Assert.Equal("ana@mail.com", single.Email);
    }

    [Fact]
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
            CreateMapper());

        var result = (await service.SearchAsync("ana")).ToList();

        var single = Assert.Single(result);
        Assert.Equal(profileId, single.ProfileId);
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
