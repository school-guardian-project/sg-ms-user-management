namespace ms_user_management.Api.Shared.Domain.Exceptions;

/// <summary>
/// El usuario autenticado no puede ver o tocar el recurso pedido: no es suyo,
/// no pertenece a su familia o esta fuera de su colegio. Es 403.
/// </summary>
public sealed class AccessDeniedException : Exception
{
    public AccessDeniedException(string message = "You are not allowed to access this resource.")
        : base(message)
    {
    }
}
