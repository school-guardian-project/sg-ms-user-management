using System.Text.Json;
using ms_user_management.Api.Shared.Application.Dto;
using Xunit;

namespace ms_user_management.Tests;

public class PersonPhoneContractTests
{
    [Fact]
    public void RequestsAcceptTenDigitPhonesAndAdministratorAssignments()
    {
        var cityId = Guid.NewGuid();
        var schoolId = Guid.NewGuid();
        var json = JsonSerializer.Serialize(new { Phone = 3001234567L, CityId = cityId, SchoolId = schoolId });
        var request = JsonSerializer.Deserialize<PersonRequestDto>(json)!;

        Assert.Equal(3001234567L, request.Phone);
        Assert.Equal(cityId, request.CityId);
        Assert.Equal(schoolId, request.SchoolId);
        Assert.Equal(typeof(long), typeof(ms_user_management.Api.Shared.Domain.Model.Person).GetProperty("Phone")!.PropertyType);
        Assert.Equal(typeof(long), typeof(ms_user_management.Api.Shared.Infrastructure.Persistence.Entity.PersonEntity).GetProperty("Phone")!.PropertyType);
    }
}
