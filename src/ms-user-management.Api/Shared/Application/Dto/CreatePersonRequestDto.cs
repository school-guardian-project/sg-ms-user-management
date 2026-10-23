namespace ms_user_management.Api.Shared.Application.Dto;

/// <summary>
/// Alta de student, driver o parent.
///
/// <para>
/// <see cref="CampusId"/> es la sede a la que pertenece la persona. El frontend
/// lo manda en el formulario de registro (dropdown de sedes) y se valida por
/// gRPC contra ms-school-management antes de guardar nada. No se guarda en
/// <c>UserManagement.Person</c>: la relacion persona-sede vive en
/// <c>Iam.Profile.CampuseId</c>, que escribe ms-iam al consumir el evento de
/// creacion.
/// </para>
/// </summary>
public class CreatePersonRequestDto : PersonRequestDto
{
    /// <summary>
    /// Sede. Opcional en el tipo pero validado como obligatorio por el caso de
    /// uso: se declara nullable para que la respuesta de validacion de
    /// <c>[ApiController]</c> diga "falta campusId" en vez de fallar al
    /// deserializar con un error ilegible.
    /// </summary>
    public Guid? CampusId { get; set; }
}
