using ms_user_management.Api.Driver.Application.Dto;
using ms_user_management.Api.Driver.Application.Security;
using ms_user_management.Api.Driver.Domain.Model;
using ms_user_management.Api.Driver.Domain.Ports.In;
using ms_user_management.Api.Family.Application.Dto;
using ms_user_management.Api.Family.Application.Security;
using ms_user_management.Api.Family.Domain.Model;
using ms_user_management.Api.Family.Domain.Ports.In;
using ms_user_management.Api.Shared.Application.Dto;
using ms_user_management.Api.Shared.Application.Security;
using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.In;
using ms_user_management.Api.Student.Application.Security;
using ms_user_management.Api.Student.Domain.Ports.In;
using ms_user_management.Tests.Fakes;
using NUnit.Framework;

namespace ms_user_management.Tests.Shared;

/// <summary>
/// Escrituras acotadas por tenant y fail-closed del filtro.
/// Escenario: el colegio S tiene la sede A; la sede B es de otro colegio.
/// El estudiante E y el acudiente P estan en A; el estudiante X y el conductor
/// D estan en B. La familia F (de A) une a P con E.
/// </summary>
public class TenantScopedWriteTests
{
    private readonly Guid _schoolS = Guid.NewGuid();
    private readonly Guid _campusA = Guid.NewGuid();
    private readonly Guid _campusB = Guid.NewGuid();
    private readonly Guid _familyF = Guid.NewGuid();

    private readonly Guid _studentPerson = Guid.NewGuid();
    private readonly Guid _studentProfile = Guid.NewGuid();
    private readonly Guid _parentPerson = Guid.NewGuid();
    private readonly Guid _parentProfile = Guid.NewGuid();
    private readonly Guid _otherStudentPerson = Guid.NewGuid();
    private readonly Guid _otherStudentProfile = Guid.NewGuid();
    private readonly Guid _otherDriverPerson = Guid.NewGuid();
    private readonly Guid _otherDriverProfile = Guid.NewGuid();
    private readonly Guid _otherAdminPerson = Guid.NewGuid();

    private InMemoryPersonProfileReader _profiles = null!;
    private InMemorySchoolCampusReader _campuses = null!;

    [SetUp]
    public void SetUp()
    {
        _profiles = new InMemoryPersonProfileReader();
        Register(_studentPerson, _studentProfile, RoleId.Student, _campusA);
        Register(_parentPerson, _parentProfile, RoleId.Parent, _campusA);
        Register(_otherStudentPerson, _otherStudentProfile, RoleId.Student, _campusB);
        Register(_otherDriverPerson, _otherDriverProfile, RoleId.Driver, _campusB);
        // Un administrador no tiene sede.
        _profiles.RoleByPerson[_otherAdminPerson] = RoleId.Admin;

        _campuses = new InMemorySchoolCampusReader();
        _campuses.CampusIdsBySchool[_schoolS] = [_campusA];
    }

    // --- Personas ---

    [Test]
    public void Admin_EditaEstudianteDeSuColegio()
    {
        Assert.DoesNotThrowAsync(() => Guard(Admin()).EnsureCanWritePersonAsync(_studentPerson, RoleId.Student));
    }

    [Test]
    public void Admin_NoEditaEstudianteDeOtroColegio()
    {
        Assert.ThrowsAsync<AccessDeniedException>(
            () => Guard(Admin()).EnsureCanWritePersonAsync(_otherStudentPerson, RoleId.Student));
    }

    [Test]
    public void SuperAdmin_EditaEstudianteDeCualquierColegio()
    {
        Assert.DoesNotThrowAsync(
            () => Guard(SuperAdmin()).EnsureCanWritePersonAsync(_otherStudentPerson, RoleId.Student));
    }

    [Test]
    public void SinUsuario_NoPuedeEscribir()
    {
        Assert.ThrowsAsync<AccessDeniedException>(
            () => Guard(new FakeCurrentUser()).EnsureCanWritePersonAsync(_studentPerson, RoleId.Student));
    }

    [Test]
    public void TokenSinRol_NoEscribeNada()
    {
        var user = new FakeCurrentUser { IsAuthenticated = true, ProfileId = Guid.NewGuid() };
        user.Permissions.Add(Permissions.StudentsManage);

        Assert.ThrowsAsync<AccessDeniedException>(
            () => Guard(user, authenticatedWithoutRole: true).EnsureCanWritePersonAsync(_studentPerson, RoleId.Student));
    }

    // --- Cambio de estado (rol desconocido) ---

    [Test]
    public void Admin_CambiaEstadoDeAcudienteDeSuColegio()
    {
        Assert.DoesNotThrowAsync(() => Guard(Admin()).EnsureCanWriteAnyPersonAsync(_parentPerson));
    }

    [Test]
    public void Admin_NoCambiaEstadoDeConductorDeOtroColegio()
    {
        Assert.ThrowsAsync<AccessDeniedException>(() => Guard(Admin()).EnsureCanWriteAnyPersonAsync(_otherDriverPerson));
    }

    [Test]
    public void Admin_NoCambiaEstadoDeOtroAdministrador()
    {
        Assert.ThrowsAsync<AccessDeniedException>(() => Guard(Admin()).EnsureCanWriteAnyPersonAsync(_otherAdminPerson));
    }

    [Test]
    public async Task DecoradorDeEstado_SoloLlamaAlCasoDeUsoSiEstaPermitido()
    {
        var inner = new RecordingStatusUseCase();
        var useCase = new AccessControlledChangePersonStatusUseCase(inner, Guard(Admin()));

        await useCase.ChangeStatusAsync(_parentPerson, "Inactive");
        Assert.ThrowsAsync<AccessDeniedException>(() => useCase.ChangeStatusAsync(_otherDriverPerson, "Inactive"));

        Assert.That(inner.Changed, Is.EqualTo(new[] { _parentPerson }));
    }

    [Test]
    public void DecoradorDeEdicion_NoLlamaAlCasoDeUsoFueraDelColegio()
    {
        var inner = new RecordingStudentWrites();
        var update = new AccessControlledUpdateStudentUseCase(inner, Guard(Admin()));
        var delete = new AccessControlledDeleteStudentUseCase(inner, Guard(Admin()));

        Assert.ThrowsAsync<AccessDeniedException>(() => update.UpdateAsync(_otherStudentPerson, new PersonRequestDto()));
        Assert.ThrowsAsync<AccessDeniedException>(() => delete.DeleteAsync(_otherStudentPerson));

        Assert.That(inner.Calls, Is.Empty);
    }

    // --- Familias ---

    [Test]
    public void Familia_NoAdmiteMiembrosDeOtroColegio()
    {
        var inner = new RecordingFamilyWrites();
        var update = new AccessControlledUpdateFamilyUseCase(inner, new FixedFamilyList(_familyF), Guard(Admin()));

        Assert.ThrowsAsync<AccessDeniedException>(() => update.ExecuteAsync(
            _familyF, "F", null, [(_parentProfile, RelationType.Parent), (_otherStudentProfile, RelationType.Student)]));
        Assert.That(inner.Calls, Is.Empty);
    }

    [Test]
    public async Task Familia_ConMiembrosDeSuColegio_SeEdita()
    {
        var inner = new RecordingFamilyWrites();
        var update = new AccessControlledUpdateFamilyUseCase(inner, new FixedFamilyList(_familyF), Guard(Admin()));

        await update.ExecuteAsync(_familyF, "F", null, [(_parentProfile, RelationType.Parent), (_studentProfile, RelationType.Student)]);

        Assert.That(inner.Calls, Is.EqualTo(new[] { _familyF }));
    }

    [Test]
    public void Familia_DeOtroColegio_NoSeBorra()
    {
        var inner = new RecordingFamilyWrites();
        var delete = new AccessControlledDeleteFamilyUseCase(inner, new FixedFamilyList(_familyF), Guard(Admin()));

        Assert.ThrowsAsync<AccessDeniedException>(() => delete.ExecuteAsync(Guid.NewGuid()));
        Assert.That(inner.Calls, Is.Empty);
    }

    // --- Licencias ---

    [Test]
    public void Licencia_DeConductorDeOtroColegio_NoSeEdita()
    {
        var licenses = new InMemoryDriverLicenseRepository();
        var license = new DriverLicense { ProfileId = _otherDriverProfile, LicenseNumber = "L-1" };
        licenses.Licenses.Add(license);
        var inner = new RecordingLicenseWrites();

        var update = new AccessControlledUpdateDriverLicenseUseCase(inner, licenses, Guard(Admin()));
        var delete = new AccessControlledDeleteDriverLicenseUseCase(inner, licenses, Guard(Admin()));

        Assert.ThrowsAsync<AccessDeniedException>(() => update.ExecuteAsync(license.Id, new UpdateDriverLicenseRequestDto()));
        Assert.ThrowsAsync<AccessDeniedException>(() => delete.ExecuteAsync(license.Id));
        Assert.That(inner.Calls, Is.Empty);
    }

    [Test]
    public async Task Licencia_Inexistente_SigueAlCasoDeUso()
    {
        var inner = new RecordingLicenseWrites();
        var delete = new AccessControlledDeleteDriverLicenseUseCase(inner, new InMemoryDriverLicenseRepository(), Guard(Admin()));

        var deleted = await delete.ExecuteAsync(Guid.NewGuid());

        Assert.That(deleted, Is.False);
        Assert.That(inner.Calls, Has.Count.EqualTo(1));
    }

    // --- Fail-closed del filtro de tenant ---

    [Test]
    public async Task Filtro_SinUsuario_NoAcota()
    {
        var filter = new TenantPersonFilter(new FakeTenantProvider(), _profiles, _campuses);

        Assert.That(await filter.GetVisibleCampusIdsAsync(), Is.Null);
    }

    [Test]
    public async Task Filtro_TokenSinRol_NoVeNinguna()
    {
        var filter = new TenantPersonFilter(
            new FakeTenantProvider { Authenticated = true, CampusId = _campusA }, _profiles, _campuses);

        Assert.That(await filter.GetVisibleCampusIdsAsync(), Is.Empty);
        Assert.That(await filter.GetVisiblePersonIdsAsync(RoleId.Student), Is.Empty);
    }

    [Test]
    public async Task Filtro_AdminSinColegio_NoVeNinguna()
    {
        var filter = new TenantPersonFilter(new FakeTenantProvider { RoleId = RoleId.Admin }, _profiles, _campuses);

        Assert.That(await filter.GetVisibleCampusIdsAsync(), Is.Empty);
        Assert.That(await filter.GetVisiblePersonIdsAsync(RoleId.Student), Is.Empty);
    }

    [Test]
    public async Task Filtro_EstudianteSinSede_NoVeNinguna()
    {
        var filter = new TenantPersonFilter(new FakeTenantProvider { RoleId = RoleId.Student }, _profiles, _campuses);

        Assert.That(await filter.GetVisibleCampusIdsAsync(), Is.Empty);
    }

    [Test]
    public async Task Filtro_SuperAdmin_NoAcota()
    {
        var filter = new TenantPersonFilter(new FakeTenantProvider { RoleId = RoleId.SuperAdmin }, _profiles, _campuses);

        Assert.That(await filter.GetVisibleCampusIdsAsync(), Is.Null);
    }

    private void Register(Guid person, Guid profile, RoleId role, Guid campus)
    {
        _profiles.ProfileByPerson[person] = profile;
        _profiles.RoleByPerson[person] = role;
        _profiles.CampusByPerson[person] = campus;
        _profiles.CampusByProfile[profile] = campus;
    }

    private PersonAccessGuard Guard(FakeCurrentUser user, bool authenticatedWithoutRole = false)
    {
        var tenant = new FakeTenantProvider
        {
            RoleId = user.RoleId,
            SchoolId = user.RoleId == RoleId.Admin ? _schoolS : null,
            CampusId = user.RoleId is RoleId.Parent or RoleId.Student ? _campusA : null,
            Authenticated = authenticatedWithoutRole ? true : null,
        };

        return new PersonAccessGuard(
            user, new TenantPersonFilter(tenant, _profiles, _campuses), _profiles, new InMemoryFamilyMembershipReader());
    }

    private static FakeCurrentUser Admin()
    {
        var user = new FakeCurrentUser { IsAuthenticated = true, RoleId = RoleId.Admin, ProfileId = Guid.NewGuid() };
        user.Permissions.Add(Permissions.StudentsManage);
        user.Permissions.Add(Permissions.UsersManage);
        return user;
    }

    private static FakeCurrentUser SuperAdmin() =>
        new() { IsAuthenticated = true, RoleId = RoleId.SuperAdmin, ProfileId = Guid.NewGuid() };

    private sealed class RecordingStatusUseCase : IChangePersonStatusUseCase
    {
        public List<Guid> Changed { get; } = new();

        public Task ChangeStatusAsync(Guid personId, string status)
        {
            Changed.Add(personId);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingStudentWrites : IUpdateStudentUseCase, IDeleteStudentUseCase
    {
        public List<Guid> Calls { get; } = new();

        public Task UpdateAsync(Guid id, PersonRequestDto dto)
        {
            Calls.Add(id);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            Calls.Add(id);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingFamilyWrites : IUpdateFamilyUseCase, IDeleteFamilyUseCase
    {
        public List<Guid> Calls { get; } = new();

        public Task ExecuteAsync(Guid familyId, string familyName, string? observations,
            IReadOnlyList<(Guid ProfileId, RelationType RelationType)> members, CancellationToken ct = default)
        {
            Calls.Add(familyId);
            return Task.CompletedTask;
        }

        public Task ExecuteAsync(Guid familyId, CancellationToken ct = default)
        {
            Calls.Add(familyId);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingLicenseWrites : IUpdateDriverLicenseUseCase, IDeleteDriverLicenseUseCase
    {
        public List<Guid> Calls { get; } = new();

        public Task<bool> ExecuteAsync(Guid id, UpdateDriverLicenseRequestDto dto, CancellationToken ct = default)
        {
            Calls.Add(id);
            return Task.FromResult(true);
        }

        public Task<bool> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            Calls.Add(id);
            return Task.FromResult(false);
        }
    }

    private sealed class FixedFamilyList(params Guid[] familyIds) : IListFamilyUseCase
    {
        public Task<IEnumerable<FamilyListDto>> ExecuteAsync(CancellationToken ct = default) =>
            Task.FromResult<IEnumerable<FamilyListDto>>(familyIds.Select(id => new FamilyListDto { Id = id }).ToList());
    }
}
