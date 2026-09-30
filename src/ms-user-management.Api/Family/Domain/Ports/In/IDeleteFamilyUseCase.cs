namespace ms_user_management.Api.Family.Domain.Ports.In;

public interface IDeleteFamilyUseCase
{
    Task ExecuteAsync(Guid familyId, CancellationToken ct = default);
}
