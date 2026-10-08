using ms_user_management.Api.Shared.Domain.Port.Out;

namespace ms_user_management.Api.Shared.Domain.Exceptions;

/// <summary>
/// ms-school-management respondio que la entidad no existe. 404: el cliente
/// puede distinguirla de un id invalido y de un servicio caido.
/// </summary>
public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(string detail)
        : base($"Entity was not found: {detail}")
    {
    }
}

/// <summary>El id de sede o colegio no tiene forma de UUID. Error del cliente.</summary>
public sealed class InvalidSchoolOrCampusIdException : Exception
{
    public InvalidSchoolOrCampusIdException(string detail)
        : base($"Invalid school or campus id: {detail}")
    {
    }
}

/// <summary>
/// ms-school-management no respondio. No se valida nada y no se da de alta a
/// nadie: es preferible un rechazo temporal a una persona registrada en una sede
/// que no existe.
/// </summary>
public sealed class SchoolDirectoryUnavailableException : Exception
{
    public SchoolDirectoryUnavailableException(string detail)
        : base($"ms-school-management is unavailable: {detail}")
    {
    }
}