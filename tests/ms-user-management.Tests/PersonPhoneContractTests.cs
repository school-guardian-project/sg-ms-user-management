using System.Text.Json;
using ms_user_management.Api.Shared.Application.Dto;
using NUnit.Framework;

namespace ms_user_management.Tests;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PersonPhoneContractTests
{
    [Test]
    public void RequestsAcceptTenDigitPhonesAndAdministratorAssignments()
    {
        var cityId = Guid.NewGuid();
        var schoolId = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new { Phone = 3001234567L, CityId = cityId, SchoolId = schoolId });
        var request = JsonSerializer.Deserialize<PersonRequestDto>(json)!;

        Assert.That(request.Phone, Is.EqualTo(3001234567L));
        Assert.That(request.CityId, Is.EqualTo(cityId));
        Assert.That(request.SchoolId, Is.EqualTo(schoolId));
        Assert.That(typeof(ms_user_management.Api.Shared.Domain.Model.Person).GetProperty("Phone")!.PropertyType, Is.EqualTo(typeof(long)));
        Assert.That(typeof(ms_user_management.Api.Shared.Infrastructure.Persistence.Entity.PersonEntity).GetProperty("Phone")!.PropertyType, Is.EqualTo(typeof(long)));
    }
}
