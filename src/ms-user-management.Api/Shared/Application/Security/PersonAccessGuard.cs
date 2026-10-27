using ms_user_management.Api.Shared.Application.Tenant;
using ms_user_management.Api.Shared.Domain.Exceptions;
using ms_user_management.Api.Shared.Domain.Model;
using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Application.Security;

/// <summary>
/// Reglas de lectura de personas y familias segun quien pregunta:
/// <list type="bullet">
/// <item>Cualquier usuario lee su propio registro.</item>
/// <item>El staff con el permiso de lectura del rol (students.read, drivers.read)
/// lee lo de su colegio; los administradores solo los lee el SuperAdmin.</item>
/// <item>Acudientes y estudiantes leen su familia y los miembros de ella.</item>
/// <item>Todo lo demas es <see cref="AccessDeniedException"/> (403).</item>
/// </list>
/// Las escrituras no pasan por aqui: las exige el permiso *.manage en el controlador.
/// </summary>
public class PersonAccessGuard
{
    private readonly ICurrentUser _user;
    private readonly TenantPersonFilter _tenantFilter;
    private readonly IPersonProfileReader _profileReader;
    private readonly IFamilyMembershipReader _families;

    public PersonAccessGuard(
        ICurrentUser user,
        TenantPersonFilter tenantFilter,
        IPersonProfileReader profileReader,
        IFamilyMembershipReader families)
    {
        _user = user;
        _tenantFilter = tenantFilter;
        _profileReader = profileReader;
        _families = families;
    }

    /// <summary>
    /// Permite leer la persona <paramref name="personId"/> registrada con el rol
    /// <paramref name="role"/> (el del endpoint: /students, /drivers...).
    /// </summary>
    public async Task EnsureCanReadPersonAsync(Guid personId, RoleId role, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (_user.PersonId == personId)
            return;

        if (IsStaffReaderOf(role))
        {
            var visible = await _tenantFilter.GetVisiblePersonIdsAsync(role, ct);
            if (visible.Contains(personId))
                return;

            throw new AccessDeniedException("The person is outside your school.");
        }

        if (IsFamilyRole && role is RoleId.Student or RoleId.Parent)
        {
            var profiles = await _profileReader.GetProfileIdsByPersonIdsAsync([personId], ct);
            if (profiles.TryGetValue(personId, out var profileId)
                && (await GetFamilyProfileIdsAsync(ct)).Contains(profileId))
                return;
        }

        throw new AccessDeniedException();
    }

    /// <summary>
    /// Alcance de un listado de personas del rol indicado: null cuando el usuario
    /// es staff (el listado ya va acotado por tenant), o los profileIds de su
    /// familia cuando es acudiente o estudiante.
    /// </summary>
    public async Task<IReadOnlySet<Guid>?> GetPersonListScopeAsync(RoleId role, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (IsStaffReaderOf(role))
            return null;

        if (IsFamilyRole && role is RoleId.Student or RoleId.Parent)
            return await GetFamilyProfileIdsAsync(ct);

        throw new AccessDeniedException();
    }

    /// <summary>
    /// Alcance de familias: null para el staff con students.read, o las familias
    /// de las que el usuario es miembro cuando es acudiente o estudiante.
    /// </summary>
    public async Task<IReadOnlySet<Guid>?> GetFamilyScopeAsync(CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (_user.HasPermission(Permissions.StudentsRead))
            return null;

        if (IsFamilyRole && _user.ProfileId is { } profileId)
            return await _families.GetFamilyIdsByMemberAsync(profileId, ct);

        throw new AccessDeniedException();
    }

    /// <summary>
    /// Permite leer los acudientes de un estudiante: el staff de su colegio o un
    /// miembro de su familia.
    /// </summary>
    public async Task EnsureCanReadStudentFamilyAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (_user.HasPermission(Permissions.StudentsRead))
        {
            var campusIds = await _tenantFilter.GetVisibleCampusIdsAsync(ct);
            if (campusIds is null
                || (await _profileReader.GetProfileIdsInCampusesAsync(campusIds, ct)).Contains(studentProfileId))
                return;

            throw new AccessDeniedException("The student is outside your school.");
        }

        if (IsFamilyRole && (await GetFamilyProfileIdsAsync(ct)).Contains(studentProfileId))
            return;

        throw new AccessDeniedException();
    }

    /// <summary>
    /// Escritura (PUT/DELETE) sobre la persona <paramref name="personId"/> del rol
    /// <paramref name="role"/>: el staff solo dentro de su colegio; el SuperAdmin
    /// sin restriccion. El permiso *.manage ya lo exige el controlador.
    /// </summary>
    public async Task EnsureCanWritePersonAsync(Guid personId, RoleId role, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (_user.RoleId == RoleId.SuperAdmin)
            return;

        var visible = await _tenantFilter.GetVisiblePersonIdsAsync(role, ct);
        if (!visible.Contains(personId))
            throw new AccessDeniedException("The person is outside your school.");
    }

    /// <summary>
    /// Escritura sobre una persona de rol desconocido (cambio de estado). Los
    /// administradores no entran: solo los gestiona el SuperAdmin.
    /// </summary>
    public async Task EnsureCanWriteAnyPersonAsync(Guid personId, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (_user.RoleId == RoleId.SuperAdmin)
            return;

        foreach (var role in TenantManagedRoles)
        {
            if ((await _tenantFilter.GetVisiblePersonIdsAsync(role, ct)).Contains(personId))
                return;
        }

        throw new AccessDeniedException("The person is outside your school.");
    }

    /// <summary>
    /// Escritura que involucra perfiles (miembros de una familia, conductor de
    /// una licencia): todos deben pertenecer a una sede del colegio del staff.
    /// </summary>
    public async Task EnsureProfilesInTenantAsync(IEnumerable<Guid> profileIds, CancellationToken ct = default)
    {
        EnsureAuthenticated();

        if (_user.RoleId == RoleId.SuperAdmin)
            return;

        var campusIds = await _tenantFilter.GetVisibleCampusIdsAsync(ct) ?? [];
        var visible = await _profileReader.GetProfileIdsInCampusesAsync(campusIds, ct);

        if (!profileIds.All(visible.Contains))
            throw new AccessDeniedException("Some profiles are outside your school.");
    }

    private static readonly RoleId[] TenantManagedRoles = [RoleId.Student, RoleId.Parent, RoleId.Driver];

    private bool IsFamilyRole => _user.RoleId is RoleId.Parent or RoleId.Student;

    private bool IsStaffReaderOf(RoleId role) => role switch
    {
        RoleId.Student or RoleId.Parent => _user.HasPermission(Permissions.StudentsRead),
        RoleId.Driver => _user.HasPermission(Permissions.DriversRead),
        // Los administradores los gestiona solo el SuperAdmin.
        _ => _user.RoleId == RoleId.SuperAdmin,
    };

    /// <summary>ProfileIds de las familias del usuario, incluido el suyo.</summary>
    private async Task<IReadOnlySet<Guid>> GetFamilyProfileIdsAsync(CancellationToken ct)
    {
        if (_user.ProfileId is not { } profileId)
            return new HashSet<Guid>();

        var familyIds = await _families.GetFamilyIdsByMemberAsync(profileId, ct);
        var members = await _families.GetMemberProfileIdsAsync(familyIds.ToList(), ct);

        return new HashSet<Guid>(members) { profileId };
    }

    /// <summary>Exige un usuario autenticado (403 si no lo hay).</summary>
    public void EnsureAuthenticated()
    {
        if (!_user.IsAuthenticated)
            throw new AccessDeniedException("Authentication required.");
    }
}
