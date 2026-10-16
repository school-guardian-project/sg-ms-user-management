namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

/// <summary>
/// Solo lectura de School.SchoolCampus (esquema School, misma base de datos):
/// Id es el Guid de la sede y SchoolId el colegio al que pertenece. Se usa para
/// expandir el schoolId del JWT del admin a sus sedes y filtrar Iam.Profile.CampuseId.
/// </summary>
public class SchoolCampusRefEntity
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
}
