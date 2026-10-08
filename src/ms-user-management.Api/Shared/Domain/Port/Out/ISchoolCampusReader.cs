namespace ms_user_management.Api.Shared.Domain.Port.Out;

/// <summary>
/// Solo lectura de School.SchoolCampus (misma base de datos): resuelve las sedes
/// de un colegio para acotar los listados al tenant del admin (schoolId del JWT).
/// </summary>
public interface ISchoolCampusReader
{
    Task<IReadOnlyCollection<Guid>> GetCampusIdsBySchoolAsync(Guid schoolId, CancellationToken ct = default);
}
