namespace ms_user_management.Api.Shared.Domain.Event;

/// <summary>
/// Se publica cuando cambia el estado de una cuenta de persona (activar o
/// desactivar). ms-iam lo consume para sincronizar <c>Iam.Profile.Status</c>, de
/// modo que la desactivacion bloquee el inicio de sesion y las asignaciones.
/// Nunca viaja la contrasena ni ningun otro dato sensible.
/// </summary>
public class PersonStatusChangedEvent
{
    public Guid EventId { get; set; }
    public Guid PersonId { get; set; }
    public string Email { get; set; }

    /// <summary>
    /// Estado nuevo de la cuenta. Se envia como cadena (<c>Active</c> /
    /// <c>Inactive</c>) para no acoplar a los consumidores al enum de C#.
    /// </summary>
    public string Status { get; set; }
}
