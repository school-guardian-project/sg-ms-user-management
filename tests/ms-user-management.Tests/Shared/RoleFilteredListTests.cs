using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using ms_user_management.Api.Admin.Application.UseCase;
using ms_user_management.Api.Driver.Application.UseCase;
using ms_user_management.Api.Parent.Application.UseCase;
using ms_user_management.Api.Shared.Application.Mapper;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Student.Application.UseCase;
using ms_user_management.Tests.Fakes;
using Xunit;

namespace ms_user_management.Tests.Shared;

/// <summary>
/// Los cuatro listados compartían el mismo defecto de origen: devolvían
/// GetAllAsync() sin filtrar, así que /api/parents, /api/students, /api/drivers
/// y /api/admins regresaban exactamente las mismas personas.
/// </summary>
public class RoleFilteredListTests
{
    private static IMapper CreateMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PersonProfile>());

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static Person Person(string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        LastName = "Perez",
        IdentificationType = default,
        IdentificationNumber = "100000" + name.Length + Guid.NewGuid().ToString("N")[..4],
        Email = name.ToLowerInvariant() + "@mail.com",
        Phone = 300111222,
        ResidenceAddress = "Calle 1",
        DateBirth = new DateOnly(1990, 1, 1),
        Status = Status.Active
    };

    /// <summary>Un repo con una persona por rol: el listado de cada rol solo debe devolver la suya.</summary>
    private static (InMemoryPersonRepository Repo, InMemoryPersonProfileReader Profiles, Guid Student, Guid Parent, Guid Driver, Guid Admin) Fixture()
    {
        var student = Person("Ana");
        var parent = Person("Luis");
        var driver = Person("Sofia");
        var admin = Person("Carlos");

        var profiles = new InMemoryPersonProfileReader();
        profiles.RoleByPerson[student.Id] = RoleId.Student;
        profiles.RoleByPerson[parent.Id] = RoleId.Parent;
        profiles.RoleByPerson[driver.Id] = RoleId.Driver;
        profiles.RoleByPerson[admin.Id] = RoleId.Admin;

        var repo = new InMemoryPersonRepository(student, parent, driver, admin);
        return (repo, profiles, student.Id, parent.Id, driver.Id, admin.Id);
    }

    [Fact]
    public async Task ListStudent_DevuelveSoloEstudiantes()
    {
        var (repo, profiles, student, _, _, _) = Fixture();

        var result = (await new ListStudentService(repo, profiles, CreateMapper()).ExecuteAsync()).ToList();

        Assert.Equal(student, Assert.Single(result).Id);
    }

    [Fact]
    public async Task ListParent_DevuelveSoloAcudientes()
    {
        var (repo, profiles, _, parent, _, _) = Fixture();

        var result = (await new ListParentService(repo, profiles, CreateMapper()).ExecuteAsync()).ToList();

        Assert.Equal(parent, Assert.Single(result).Id);
    }

    [Fact]
    public async Task ListDriver_DevuelveSoloConductores()
    {
        var (repo, profiles, _, _, driver, _) = Fixture();

        var service = new ListDriverService(repo, new InMemoryPersonLicenseReader(), profiles, CreateMapper());
        var result = (await service.ExecuteAsync()).ToList();

        Assert.Equal(driver, Assert.Single(result).Id);
    }

    [Fact]
    public async Task ListAdmin_DevuelveSoloAdministradores()
    {
        var (repo, profiles, _, _, _, admin) = Fixture();

        var result = (await new ListAdminService(repo, profiles, CreateMapper()).ExecuteAsync()).ToList();

        Assert.Equal(admin, Assert.Single(result).Id);
    }
}
