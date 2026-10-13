using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Driver;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class DriverLicenseServiceTests
{
    private static DriverLicenseRequestDto ValidRequest(Guid? profileId = null) => new()
    {
        ProfileId = profileId ?? Guid.NewGuid(),
        LicenseNumber = "ABC12345",
        LicenseExpirationDate = new DateOnly(2030, 12, 31)
    };

    [Test]
    public async Task Create_ConDatosValidos_GuardaLicenciaActiva()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);
        var profileId = Guid.NewGuid();

        var id = await service.ExecuteAsync(ValidRequest(profileId));

        var storedItems = repo.Licenses;
        Assert.That(storedItems, Has.Exactly(1).Items);
        var stored = storedItems.Single();
        Assert.That(stored.Id, Is.EqualTo(id));
        Assert.That(stored.ProfileId, Is.EqualTo(profileId));
        Assert.That(stored.LicenseNumber, Is.EqualTo("ABC12345"));
        Assert.That(stored.Status, Is.EqualTo(Status.Active));
    }

    [Test]
    public async Task Create_SinProfileId_Rechaza()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);

        Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(ValidRequest(Guid.Empty)));

        Assert.That(repo.Licenses, Is.Empty);
    }

    [Test]
    public async Task Create_ConNumeroDeLicenciaRepetido_Rechaza()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);
        await service.ExecuteAsync(ValidRequest());

        var ex = Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(ValidRequest()));

        Assert.That(ex.Message, Does.Contain("already exists"));
        Assert.That(repo.Licenses, Has.Exactly(1).Items);
    }

    [Test]
    public async Task Create_SinFechaDeExpiracion_Rechaza()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);
        var dto = ValidRequest();
        dto.LicenseExpirationDate = default;

        Assert.ThrowsAsync<ArgumentException>(() => service.ExecuteAsync(dto));

        Assert.That(repo.Licenses, Is.Empty);
    }

    [Test]
    public async Task List_FiltraPorProfileId()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var wanted = Guid.NewGuid();
        await new CreateDriverLicenseService(repo).ExecuteAsync(ValidRequest(wanted));

        var otro = ValidRequest();
        otro.LicenseNumber = "OTHER1234";
        await new CreateDriverLicenseService(repo).ExecuteAsync(otro);

        var result = await new ListDriverLicenseService(repo).ExecuteAsync(wanted);

        var rowItems = result;
        Assert.That(rowItems, Has.Exactly(1).Items);
        var row = rowItems.Single();
        Assert.That(row.ProfileId, Is.EqualTo(wanted));
    }

    [Test]
    public async Task Update_YDelete_SobreLicenciaExistente()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var id = await new CreateDriverLicenseService(repo).ExecuteAsync(ValidRequest());

        var updated = await new UpdateDriverLicenseService(repo).ExecuteAsync(id,
            new UpdateDriverLicenseRequestDto
            {
                LicenseNumber = "XYZ99999",
                LicenseExpirationDate = new DateOnly(2031, 1, 1),
                Status = nameof(Status.Inactive)
            });

        Assert.That(updated, Is.True);
        Assert.That(repo.Licenses.Single().LicenseNumber, Is.EqualTo("XYZ99999"));
        Assert.That(repo.Licenses.Single().Status, Is.EqualTo(Status.Inactive));

        Assert.That(await new DeleteDriverLicenseService(repo).ExecuteAsync(id), Is.True);
        Assert.That(await new DeleteDriverLicenseService(repo).ExecuteAsync(id), Is.False);
        Assert.That(repo.Licenses, Is.Empty);
    }

    [Test]
    public async Task Update_YGet_SobreLicenciaInexistente_NoEncuentra()
    {
        var repo = new InMemoryDriverLicenseRepository();

        Assert.That(await new UpdateDriverLicenseService(repo).ExecuteAsync(
            Guid.NewGuid(), new UpdateDriverLicenseRequestDto
            {
                LicenseNumber = "ABC12345",
                LicenseExpirationDate = new DateOnly(2030, 12, 31)
            }), Is.False);

        Assert.That(await new GetDriverLicenseService(repo).ExecuteAsync(Guid.NewGuid()), Is.Null);
    }
}
