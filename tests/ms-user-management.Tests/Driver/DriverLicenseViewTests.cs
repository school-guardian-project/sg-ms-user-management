using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Driver;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
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

    [Test]
    public async Task List_ConLicencia_RellenaLicenciaYEmail()
    {
        var withLicense = Guid.NewGuid();
        var withoutLicense = Guid.NewGuid();
        var personRepo = new InMemoryPersonRepository(
            Driver(withLicense, "Ana", "ana@mail.com"),
            Driver(withoutLicense, "Luis", "luis@mail.com"));

        var reader = new InMemoryPersonLicenseReader();
        reader.LicensesByPerson[withLicense] = LicenseFor(withLicense);

        var profileReader = new InMemoryPersonProfileReader();
        profileReader.RoleByPerson[withLicense] = RoleId.Driver;
        profileReader.RoleByPerson[withoutLicense] = RoleId.Driver;

        var service = new ListDriverService(personRepo, reader, profileReader, TenantFakes.NoFilter(profileReader), CreateMapper());

        var result = (await service.ExecuteAsync()).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        var ana = result.Single(r => r.Id == withLicense);
        Assert.That(ana.LicenseNumber, Is.EqualTo("ABC12345"));
        Assert.That(ana.LicenseExpirationDate, Is.EqualTo(new DateOnly(2030, 12, 31)));
        Assert.That(ana.Email, Is.EqualTo("ana@mail.com"));

        var luis = result.Single(r => r.Id == withoutLicense);
        Assert.That(luis.LicenseNumber, Is.Null);
        Assert.That(luis.LicenseExpirationDate, Is.Null);
    }

    [Test]
    public async Task Get_ConLicencia_RellenaLicenciaEnDetalle()
    {
        var personId = Guid.NewGuid();
        var personRepo = new InMemoryPersonRepository(Driver(personId, "Ana", "ana@mail.com"));

        var reader = new InMemoryPersonLicenseReader();
        reader.LicensesByPerson[personId] = LicenseFor(personId);

        var service = new GetDriverService(personRepo, reader, CreateMapper());

        var detail = await service.GetByIdAsync(personId);

        Assert.That(detail.LicenseNumber, Is.EqualTo("ABC12345"));
        Assert.That(detail.LicenseExpirationDate, Is.EqualTo(new DateOnly(2030, 12, 31)));
        Assert.That(detail.Email, Is.EqualTo("ana@mail.com"));
    }
}
