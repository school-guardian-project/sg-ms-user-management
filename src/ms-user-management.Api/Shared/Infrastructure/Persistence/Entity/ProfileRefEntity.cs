namespace ms_user_management.Api.Shared.Infrastructure.Persistence.Entity;

/// <summary>
/// Solo lectura de Iam.Profile (esquema Iam, misma base de datos): la licencia
/// del conductor se guarda por ProfileId y la API trabaja con PersonId.
/// </summary>
public class ProfileRefEntity
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
}
