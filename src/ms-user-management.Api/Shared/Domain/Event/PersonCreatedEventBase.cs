namespace ms_user_management.Api.Shared.Domain.Event;

/// <summary>
/// Base de los eventos de persona creada. ms-iam los consume para crear el
/// <c>Iam.Profile</c> (rol, contrasena y sede) y desde ahi se resuelve todo lo
/// que el frontend necesita para iniciar sesion.
/// </summary>
public abstract class PersonCreatedEventBase
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public string Email { get; set; }
    public string IdentificationNumber { get; set; }

    /// <summary>
    /// Sede de la persona. Viaja en student.created, driver.created y
    /// parent.created; queda null en admin.created, porque un admin se relaciona
    /// con un colegio y no con una sede.
    ///
    /// <para>
    /// Nullable a proposito: el consumidor debe seguir funcionando con eventos
    /// publicados antes de que existiera el campo.
    /// </para>
    /// </summary>
    public Guid? CampusId { get; set; }

    /// <summary>
    /// Colegio que administra el admin. Solo viaja en admin.created. ms-iam lo
    /// usa para registrar la relacion en <c>School.SchoolAdmin</c> por gRPC.
    /// </summary>
    public Guid? SchoolId { get; set; }
}