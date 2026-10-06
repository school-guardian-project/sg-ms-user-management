using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;

namespace ms_user_management.Tests.Driver;

public class DriverLicenseServiceTests
{
    private static DriverLicenseRequestDto ValidRequest(Guid? profileId = null) => new()
    {
        ProfileId = profileId ?? Guid.NewGuid(),
        LicenseNumber = "ABC12345",
        LicenseExpirationDate = new DateOnly(2030, 12, 31)
    };

    [Fact]
    public async Task Create_ConDatosValidos_GuardaLicenciaActiva()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);
        var profileId = Guid.NewGuid();

        var id = await service.ExecuteAsync(ValidRequest(profileId));

        var stored = Assert.Single(repo.Licenses);
        Assert.Equal(id, stored.Id);
        Assert.Equal(profileId, stored.ProfileId);
        Assert.Equal("ABC12345", stored.LicenseNumber);
        Assert.Equal(Status.Active, stored.Status);
    }

    [Fact]
    public async Task Create_SinProfileId_Rechaza()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ExecuteAsync(ValidRequest(Guid.Empty)));

        Assert.Empty(repo.Licenses);
    }

    [Fact]
    public async Task Create_ConNumeroDeLicenciaRepetido_Rechaza()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);
        await service.ExecuteAsync(ValidRequest());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ExecuteAsync(ValidRequest()));

        Assert.Contains("already exists", ex.Message);
        Assert.Single(repo.Licenses);
    }

    [Fact]
    public async Task Create_SinFechaDeExpiracion_Rechaza()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var service = new CreateDriverLicenseService(repo);
        var dto = ValidRequest();
        dto.LicenseExpirationDate = default;

        await Assert.ThrowsAsync<ArgumentException>(() => service.ExecuteAsync(dto));

        Assert.Empty(repo.Licenses);
    }

    [Fact]
    public async Task List_FiltraPorProfileId()
    {
        var repo = new InMemoryDriverLicenseRepository();
        var wanted = Guid.NewGuid();
        await new CreateDriverLicenseService(repo).ExecuteAsync(ValidRequest(wanted));

        var otro = ValidRequest();
        otro.LicenseNumber = "OTHER1234";
        await new CreateDriverLicenseService(repo).ExecuteAsync(otro);

        var result = await new ListDriverLicenseService(repo).ExecuteAsync(wanted);

        var row = Assert.Single(result);
        Assert.Equal(wanted, row.ProfileId);
    }

    [Fact]
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

        Assert.True(updated);
        Assert.Equal("XYZ99999", repo.Licenses.Single().LicenseNumber);
        Assert.Equal(Status.Inactive, repo.Licenses.Single().Status);

        Assert.True(await new DeleteDriverLicenseService(repo).ExecuteAsync(id));
        Assert.False(await new DeleteDriverLicenseService(repo).ExecuteAsync(id));
        Assert.Empty(repo.Licenses);
    }

    [Fact]
    public async Task Update_YGet_SobreLicenciaInexistente_NoEncuentra()
    {
        var repo = new InMemoryDriverLicenseRepository();

        Assert.False(await new UpdateDriverLicenseService(repo).ExecuteAsync(
            Guid.NewGuid(), new UpdateDriverLicenseRequestDto
            {
                LicenseNumber = "ABC12345",
                LicenseExpirationDate = new DateOnly(2030, 12, 31)
            }));

        Assert.Null(await new GetDriverLicenseService(repo).ExecuteAsync(Guid.NewGuid()));
    }
}
