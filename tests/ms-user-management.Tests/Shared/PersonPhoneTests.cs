using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class PersonPhoneTests
{
    [Test]
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

        Assert.That(result.Id, Is.EqualTo(person.Id));
        Assert.That(result.Phone, Is.EqualTo(3001234567L));
        Assert.That(result.Name, Is.EqualTo(person.Name));
        Assert.That(result.Email, Is.EqualTo(person.Email));
        Assert.That(result.ResidenceAddress, Is.EqualTo(person.ResidenceAddress));
        Assert.That(result.DateBirth, Is.EqualTo(person.DateBirth));
    }
}
