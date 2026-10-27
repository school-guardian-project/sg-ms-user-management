namespace ms_user_management.Api.Driver.Domain.Ports.In;

public interface IDeleteDriverLicenseUseCase
{
    /// <returns>false si la licencia no existe.</returns>
    Task<bool> ExecuteAsync(Guid id, CancellationToken ct = default);
}
