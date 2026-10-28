namespace ms_user_management.Api.Driver.Domain.Ports.Out;

public interface IPersonLicenseWriter
{
    /// <summary>
    /// Crea o actualiza la licencia del conductor. Devuelve false si la persona
    /// todavia no tiene perfil (el perfil lo crea IAM de forma asincrona).
    /// </summary>
    Task<bool> UpsertByPersonIdAsync(
        Guid personId,
        string licenseNumber,
        DateOnly expirationDate,
        CancellationToken ct = default);
}
