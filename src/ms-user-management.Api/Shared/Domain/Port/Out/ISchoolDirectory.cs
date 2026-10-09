namespace ms_user_management.Api.Shared.Domain.Port.Out;

/// <summary>
/// Datos de colegio y sede que ms-user-management necesita al dar de alta
/// personas y que viven en el esquema <c>School</c>, propiedad de otro servicio.
/// Se resuelve por gRPC en vez de leer su base de datos.
/// </summary>
public interface ISchoolDirectory
{
    /// <summary>
    /// Devuelve el nombre del colegio, o <c>null</c> si el id no existe. El
    /// nombre existe solo para poder responder con un mensaje util: "el colegio
    /// X no existe" y no "el id no es valido".
    /// </summary>
    Task<string?> FindSchoolNameAsync(Guid schoolId, CancellationToken ct = default);

    /// <summary>
    /// Devuelve el nombre de la sede, o <c>null</c> si el id no existe o esta
    /// inactiva. Una sede inactiva no admite altas nuevas.
    /// </summary>
    Task<string?> FindCampusNameAsync(Guid campusId, CancellationToken ct = default);

    /// <summary>
    /// Lanza <see cref="SchoolNotFoundException"/> si el colegio no existe y
    /// <see cref="CampusNotFoundException"/> si la sede no. Los casos de uso los
    /// usan para responder 400 con un mensaje que el frontend pueda mostrar.
    /// </summary>
    Task EnsureSchoolExistsAsync(Guid schoolId, CancellationToken ct = default);

    Task EnsureCampusExistsAsync(Guid campusId, CancellationToken ct = default);
}

/// <summary>
/// El id de colegio que envio el frontend no corresponde a ningun colegio. Es un
/// dato invalido del request, no una falla del sistema: 400.
/// </summary>
public sealed class SchoolNotFoundException(Guid schoolId)
    : Exception($"School {schoolId} was not found");

/// <summary>
/// El id de sede que envio el frontend no corresponde a ninguna sede activa.
/// Mismo criterio que <see cref="SchoolNotFoundException"/>: 400.
/// </summary>
public sealed class CampusNotFoundException(Guid campusId)
    : Exception($"Campus {campusId} was not found");