using Microsoft.AspNetCore.Mvc;
using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Application.Security;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Infrastructure.Controller;
using ms_user_management.Api.Student.Application.Security;
using ms_user_management.Api.Student.Domain.Ports.In;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

/// <summary>
/// Escenario: el colegio S tiene la sede A; la sede B es de otro colegio.
/// La familia F une al acudiente P con el estudiante E (ambos en la sede A).
/// El estudiante X esta en la sede B y no es de la familia.
/// </summary>
public class PersonAccessGuardTests
{
    private readonly Guid _schoolS = Guid.NewGuid();
    private readonly Guid _campusA = Guid.NewGuid();
    private readonly Guid _campusB = Guid.NewGuid();
    private readonly Guid _familyF = Guid.NewGuid();

    private readonly Guid _parentPerson = Guid.NewGuid();
    private readonly Guid _parentProfile = Guid.NewGuid();
    private readonly Guid _studentPerson = Guid.NewGuid();
    private readonly Guid _studentProfile = Guid.NewGuid();
    private readonly Guid _otherStudentPerson = Guid.NewGuid();
    private readonly Guid _otherStudentProfile = Guid.NewGuid();
    private readonly Guid _adminPerson = Guid.NewGuid();
    private readonly Guid _otherAdminPerson = Guid.NewGuid();

    private InMemoryPersonProfileReader _profiles = null!;
    private InMemoryFamilyMembershipReader _families = null!;
    private InMemorySchoolCampusReader _campuses = null!;

    [SetUp]
    public void SetUp()
    {
        _profiles = new InMemoryPersonProfileReader();
        Register(_parentPerson, _parentProfile, RoleId.Parent, _campusA);
        Register(_studentPerson, _studentProfile, RoleId.Student, _campusA);
        Register(_otherStudentPerson, _otherStudentProfile, RoleId.Student, _campusB);
        _profiles.RoleByPerson[_adminPerson] = RoleId.Admin;
        _profiles.RoleByPerson[_otherAdminPerson] = RoleId.Admin;

        _families = new InMemoryFamilyMembershipReader();
        _families.Members.Add((_familyF, _parentProfile));
        _families.Members.Add((_familyF, _studentProfile));

        _campuses = new InMemorySchoolCampusReader();
        _campuses.CampusIdsBySchool[_schoolS] = [_campusA];
    }

    // --- Lectura de una persona ---

    [Test]
    public void CualquierUsuario_LeeSuPropioRegistro()
    {
        var guard = Guard(Parent());

        Assert.DoesNotThrowAsync(() => guard.EnsureCanReadPersonAsync(_parentPerson, RoleId.Parent));
    }

    [Test]
    public void AdminConPermiso_LeeEstudianteDeSuColegio()
    {
        var guard = Guard(Admin(Permissions.StudentsRead));

        Assert.DoesNotThrowAsync(() => guard.EnsureCanReadPersonAsync(_studentPerson, RoleId.Student));
    }

    [Test]
    public void AdminConPermiso_NoLeeEstudianteDeOtroColegio()
    {
        var guard = Guard(Admin(Permissions.StudentsRead));

        Assert.ThrowsAsync<AccessDeniedException>(
            () => guard.EnsureCanReadPersonAsync(_otherStudentPerson, RoleId.Student));
    }

    [Test]
    public void AdminSinPermisoDeLectura_NoLeeEstudiantes()
    {
        var guard = Guard(Admin());

        Assert.ThrowsAsync<AccessDeniedException>(
            () => guard.EnsureCanReadPersonAsync(_studentPerson, RoleId.Student));
    }

    [Test]
    public void Acudiente_LeeAlEstudianteDeSuFamilia()
    {
        var guard = Guard(Parent());

        Assert.DoesNotThrowAsync(() => guard.EnsureCanReadPersonAsync(_studentPerson, RoleId.Student));
    }

    [Test]
    public void Acudiente_NoLeeEstudianteAjeno()
    {
        var guard = Guard(Parent());

        Assert.ThrowsAsync<AccessDeniedException>(
            () => guard.EnsureCanReadPersonAsync(_otherStudentPerson, RoleId.Student));
    }

    [Test]
    public void Estudiante_LeeAlAcudienteDeSuFamilia()
    {
        var guard = Guard(Student());

        Assert.DoesNotThrowAsync(() => guard.EnsureCanReadPersonAsync(_parentPerson, RoleId.Parent));
    }

    [Test]
    public void Conductor_NoLeeEstudiantes()
    {
        var guard = Guard(User(RoleId.Driver, Guid.NewGuid(), Guid.NewGuid()));

        Assert.ThrowsAsync<AccessDeniedException>(
            () => guard.EnsureCanReadPersonAsync(_studentPerson, RoleId.Student));
    }

    [Test]
    public void Admin_LeeSuPropioRegistroPeroNoElDeOtroAdmin()
    {
        var guard = Guard(Admin(Permissions.UsersRead));

        Assert.DoesNotThrowAsync(() => guard.EnsureCanReadPersonAsync(_adminPerson, RoleId.Admin));
        Assert.ThrowsAsync<AccessDeniedException>(
            () => guard.EnsureCanReadPersonAsync(_otherAdminPerson, RoleId.Admin));
    }

    [Test]
    public void SuperAdmin_LeeCualquierAdmin()
    {
        var guard = Guard(User(RoleId.SuperAdmin, Guid.NewGuid(), Guid.NewGuid()));

        Assert.DoesNotThrowAsync(() => guard.EnsureCanReadPersonAsync(_otherAdminPerson, RoleId.Admin));
    }

    [Test]
    public void SinAutenticar_SeRechaza()
    {
        var guard = Guard(new FakeCurrentUser());

        Assert.ThrowsAsync<AccessDeniedException>(
            () => guard.EnsureCanReadPersonAsync(_studentPerson, RoleId.Student));
        Assert.ThrowsAsync<AccessDeniedException>(() => guard.GetFamilyScopeAsync());
    }

    // --- Alcance de listados ---

    [Test]
    public async Task Listado_StaffSinAcotar_AcudienteSoloSuFamilia()
    {
        Assert.That(await Guard(Admin(Permissions.StudentsRead)).GetPersonListScopeAsync(RoleId.Student), Is.Null);

        var scope = await Guard(Parent()).GetPersonListScopeAsync(RoleId.Student);

        Assert.That(scope, Is.EquivalentTo(new[] { _parentProfile, _studentProfile }));
    }

    [Test]
    public void Listado_ConductorNoListaEstudiantes()
    {
        var guard = Guard(User(RoleId.Driver, Guid.NewGuid(), Guid.NewGuid()));

        Assert.ThrowsAsync<AccessDeniedException>(() => guard.GetPersonListScopeAsync(RoleId.Student));
    }

    [Test]
    public async Task Familias_StaffSinAcotar_AcudienteSoloLasSuyas()
    {
        Assert.That(await Guard(Admin(Permissions.StudentsRead)).GetFamilyScopeAsync(), Is.Null);
        Assert.That(await Guard(Parent()).GetFamilyScopeAsync(), Is.EquivalentTo(new[] { _familyF }));
    }

    [Test]
    public void AcudientesDeUnEstudiante_MiembroSi_StaffDeOtroColegioNo()
    {
        Assert.DoesNotThrowAsync(() => Guard(Parent()).EnsureCanReadStudentFamilyAsync(_studentProfile));
        Assert.ThrowsAsync<AccessDeniedException>(
            () => Guard(Parent()).EnsureCanReadStudentFamilyAsync(_otherStudentProfile));
        Assert.ThrowsAsync<AccessDeniedException>(
            () => Guard(Admin(Permissions.StudentsRead)).EnsureCanReadStudentFamilyAsync(_otherStudentProfile));
    }

    // --- Decoradores ---

    [Test]
    public async Task ListadoDeEstudiantes_ParaAcudiente_SoloDevuelveSusHijos()
    {
        var inner = new FixedStudentList(
            new PersonListDto { Id = _studentPerson, ProfileId = _studentProfile },
            new PersonListDto { Id = _otherStudentPerson, ProfileId = _otherStudentProfile });

        var result = (await new AccessControlledListStudentUseCase(inner, Guard(Parent())).ExecuteAsync()).ToList();

        Assert.That(result.Select(p => p.Id), Is.EqualTo(new[] { _studentPerson }));
    }

    [Test]
    public async Task ListadoDeEstudiantes_ParaStaff_NoSeRecorta()
    {
        var inner = new FixedStudentList(
            new PersonListDto { Id = _studentPerson, ProfileId = _studentProfile },
            new PersonListDto { Id = _otherStudentPerson, ProfileId = _otherStudentProfile });

        var result = await new AccessControlledListStudentUseCase(inner, Guard(Admin(Permissions.StudentsRead))).ExecuteAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task DetalleDeFamilia_MiembroSi_AjenoNo()
    {
        var otherFamily = Guid.NewGuid();
        var tenantFamilies = new FixedFamilyList(_familyF, otherFamily);
        var inner = new FixedFamilyDetail();

        var asParent = new AccessControlledGetFamilyUseCase(inner, tenantFamilies, Guard(Parent()));
        Assert.That(await asParent.ExecuteAsync(_familyF), Is.Not.Null);
        Assert.ThrowsAsync<AccessDeniedException>(() => asParent.ExecuteAsync(otherFamily));
    }

    [Test]
    public void DetalleDeFamilia_StaffSoloSiEstaEnSuListadoDeColegio()
    {
        var outsideFamily = Guid.NewGuid();
        var useCase = new AccessControlledGetFamilyUseCase(
            new FixedFamilyDetail(), new FixedFamilyList(_familyF), Guard(Admin(Permissions.StudentsRead)));

        Assert.DoesNotThrowAsync(() => useCase.ExecuteAsync(_familyF));
        Assert.ThrowsAsync<AccessDeniedException>(() => useCase.ExecuteAsync(outsideFamily));
    }

    [Test]
    public void ApiErrors_AccesoDenegado_Devuelve403()
    {
        var result = ApiErrors.ToProblem(new AccessDeniedException());

        Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(403));
    }

    // --- Helpers ---

    private void Register(Guid person, Guid profile, RoleId role, Guid campus)
    {
        _profiles.ProfileByPerson[person] = profile;
        _profiles.RoleByPerson[person] = role;
        _profiles.CampusByPerson[person] = campus;
        _profiles.CampusByProfile[profile] = campus;
    }

    private PersonAccessGuard Guard(FakeCurrentUser user)
    {
        var tenant = new FakeTenantProvider
        {
            RoleId = user.RoleId,
            SchoolId = user.RoleId == RoleId.Admin ? _schoolS : null,
            CampusId = user.RoleId is RoleId.Parent or RoleId.Student ? _campusA : null,
        };

        return new PersonAccessGuard(
            user, new TenantPersonFilter(tenant, _profiles, _campuses), _profiles, _families);
    }

    private FakeCurrentUser Admin(params string[] permissions)
    {
        var user = User(RoleId.Admin, Guid.NewGuid(), _adminPerson);
        foreach (var permission in permissions)
            user.Permissions.Add(permission);
        return user;
    }

    private FakeCurrentUser Parent() => User(RoleId.Parent, _parentProfile, _parentPerson);

    private FakeCurrentUser Student() => User(RoleId.Student, _studentProfile, _studentPerson);

    private static FakeCurrentUser User(RoleId role, Guid profileId, Guid personId) => new()
    {
        IsAuthenticated = true,
        RoleId = role,
        ProfileId = profileId,
        PersonId = personId,
    };

    private sealed class FixedStudentList(params PersonListDto[] persons) : IListStudentUseCase
    {
        public Task<IEnumerable<PersonListDto>> ExecuteAsync() => Task.FromResult<IEnumerable<PersonListDto>>(persons);
    }

    private sealed class FixedFamilyList(params Guid[] familyIds) : IListFamilyUseCase
    {
        public Task<IEnumerable<FamilyListDto>> ExecuteAsync(CancellationToken ct = default) =>
            Task.FromResult<IEnumerable<FamilyListDto>>(familyIds.Select(id => new FamilyListDto { Id = id }).ToList());
    }

    private sealed class FixedFamilyDetail : IGetFamilyUseCase
    {
        public Task<FamilyResponseDto> ExecuteAsync(Guid familyId, CancellationToken ct = default) =>
            Task.FromResult(new FamilyResponseDto());
    }
}
