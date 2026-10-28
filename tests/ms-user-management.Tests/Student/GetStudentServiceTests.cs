using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Student;

public class GetStudentServiceTests
{
    [TestCase(true)]
    [TestCase(false)]
    public async Task GetById_ReturnsAssociatedProfileOrNullWithoutUsingPersonId(bool hasProfile)
    {
        var person = new Person
        {
            Id = Guid.NewGuid(), Name = "Ana", LastName = "Perez",
            IdentificationNumber = "123456", Email = "ana@example.com",
            Phone = 3001234567, ResidenceAddress = "Calle 1",
            DateBirth = new DateOnly(2010, 4, 15)
        };
        var profileId = Guid.NewGuid();
        var profiles = new InMemoryPersonProfileReader();
        if (hasProfile)
            profiles.ProfileByPerson[person.Id] = profileId;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PersonProfile>());
        using var provider = services.BuildServiceProvider();
        var service = new GetStudentService(
            new InMemoryPersonRepository(person), provider.GetRequiredService<IMapper>(), profiles);

        var result = await service.GetByIdAsync(person.Id);

        Assert.That(result.Id, Is.EqualTo(person.Id));
        Assert.That(result.Name, Is.EqualTo(person.Name));
        Assert.That(result.ProfileId, Is.EqualTo(hasProfile ? (Guid?)profileId : null));
        Assert.That(result.ProfileId, Is.Not.EqualTo(person.Id));
    }
}
