namespace ms_user_management.Api.Admin.Domain.Ports.In;

public interface IDeleteAdminUseCase
{
    Task DeleteAsync(Guid id);
}
