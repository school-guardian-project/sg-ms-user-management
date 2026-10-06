using ms_user_management.Api.Driver.Domain.Ports.Out;

namespace ms_user_management.Api.Driver.Application.UseCase;

public class DeleteDriverLicenseService
{
    private readonly IDriverLicenseRepository _repository;

    public DeleteDriverLicenseService(IDriverLicenseRepository repository)
    {
        _repository = repository;
    }

    public Task<bool> ExecuteAsync(Guid id, CancellationToken ct = default)
        => _repository.DeleteAsync(id, ct);
}
