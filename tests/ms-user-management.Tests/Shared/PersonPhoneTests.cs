using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using Xunit;

namespace ms_user_management.Tests.Shared;

public class PersonPhoneTests
{
    [Fact]
    public async Task GetAdmin_PreservesBigIntPhoneAndPersonalDetails()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PersonProfile>());
        using var provider = services.BuildServiceProvider();
        var person = new Person
        {
            Id = Guid.NewGuid(),
            Name = "Current", LastName = "User",
            IdentificationType = default, IdentificationNumber = "123456789",
            Email = "current@example.invalid", Phone = 3001234567L,
            ResidenceAddress = "Current Address", DateBirth = new DateOnly(1990, 5, 10),
            Status = Status.Active
        };
        var service = new GetAdminService(new InMemoryPersonRepository(person),
            provider.GetRequiredService<IMapper>());

        var result = await service.GetByIdAsync(person.Id);

        Assert.Equal(person.Id, result.Id);
        Assert.Equal(3001234567L, result.Phone);
        Assert.Equal(person.Name, result.Name);
        Assert.Equal(person.Email, result.Email);
        Assert.Equal(person.ResidenceAddress, result.ResidenceAddress);
        Assert.Equal(person.DateBirth, result.DateBirth);
    }
}
