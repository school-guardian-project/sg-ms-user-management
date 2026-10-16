using ms_user_management.Api.Shared.Domain.Model;

namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

/// <summary>
/// Solo lectura de Iam.Profile (esquema Iam, misma base de datos): la licencia
/// del conductor se guarda por ProfileId y la API trabaja con PersonId.
/// RoleId es lo que permite que /api/parents, /api/students, /api/drivers y
/// /api/admins devuelvan cada uno solo su rol.
/// </summary>
public class ProfileRefEntity
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public RoleId RoleId { get; set; }

    /// <summary>Sede del perfil (nullable). Se usa para acotar los listados al tenant del JWT.</summary>
    public Guid? CampuseId { get; set; }
}
