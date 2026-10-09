namespace ms_user_management.Api.Shared.Domain.Exceptions;

/// <summary>
/// El frontend no mando <c>campusId</c>. Es 400 con un mensaje que el formulario
/// puede mostrar, en vez de un error de deserializacion que el cliente no sabe
/// interpretar.
/// </summary>
public sealed class CampusIdRequiredException : Exception
{
    public CampusIdRequiredException(string role)
        : base($"campusId is required to register a {role}")
    {
    }
}

/// <summary>
/// El frontend no mando <c>schoolId</c>. Mismo criterio que
/// <see cref="CampusIdRequiredException"/>: un admin sin colegio no puede operar.
/// </summary>
public sealed class SchoolIdRequiredException : Exception
{
    public SchoolIdRequiredException()
        : base("schoolId is required to register an admin")
    {
    }
}