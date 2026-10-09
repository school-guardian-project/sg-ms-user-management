namespace ms_user_management.Api.Shared.Application.Dto;

/// <summary>
/// Datos comunes de una persona. Se mantiene aparte de los DTOs de alta
/// porque <c>PUT</c> no lleva <c>campusId</c>: la sede se asigna al dar de alta
/// y cambiarla es un caso distinto (un traslado de sede), no un Simple update.
/// </summary>
public class PersonRequestDto
{
    public string Name { get; set; }
    public string LastName { get; set; }
    public string IdentificationType { get; set; }
    public string IdentificationNumber { get; set; }
    public string Email { get; set; }
    public long Phone { get; set; }
    public string ResidenceAddress { get; set; }
    public DateOnly DateBirth { get; set; }
}

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

/// <summary>
/// Alta de admin.
///
/// <para>
/// Un admin pertenece a un <em>colegio</em>, no a una sede: por eso esta clase
/// lleva <see cref="SchoolId"/> y no <c>CampusId</c>. La relacion se materializa
/// en <c>School.SchoolAdmin</c>, que ms-iam escribe por gRPC despues de crear el
/// perfil.
/// </para>
/// </summary>
public class CreateAdminRequestDto : PersonRequestDto
{
    public Guid? SchoolId { get; set; }
}