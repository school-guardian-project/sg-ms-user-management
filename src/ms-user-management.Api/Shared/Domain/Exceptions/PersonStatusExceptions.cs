namespace ms_user_management.Api.Shared.Domain.Exceptions;

/// <summary>
/// La persona no existe. Es 404: el cliente puede distinguirlo de un id mal
/// formado (400) y de un servicio caido (503).
/// </summary>
public sealed class PersonNotFoundException : Exception
{
    public PersonNotFoundException(Guid id)
        : base($"Person {id} was not found")
    {
    }
}

/// <summary>
/// El estado enviado no es <c>Active</c> ni <c>Inactive</c>. Es un dato del
/// formulario, asi que es 400 con un mensaje que la pantalla puede mostrar.
/// </summary>
public sealed class InvalidPersonStatusException : Exception
{
    public InvalidPersonStatusException(string value)
        : base($"Invalid status '{value}'. Allowed values: Active, Inactive")
    {
    }
}
